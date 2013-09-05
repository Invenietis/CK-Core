using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using CK.RouteConfig;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    public class ConfigHostTests
    {

        /// <summary>
        /// Mocks a stupid, non concurrent, file in append mode that appends strings into a list.
        /// </summary>
        class File
        {
            public List<string> Content;
            bool _isOpened;
            bool _writing;
            bool _openingOrClosing;

            /// <summary>
            /// Drives the Thread.Sleep calls. When -1, no Thread.Sleep is done.
            /// </summary>
            static public int FileWaitMilliSeconds = -1;


            static public readonly Dictionary<string,File> AllFiles = new Dictionary<string, File>();

            /// <summary>
            /// Since concurrency is checked in Open, we do not check it here and use a 
            /// simple non concurrent dictionary.
            /// </summary>
            static public File OpenOrCreate( string fileName )
            {
                File f = AllFiles.GetOrSet( fileName, n => new File() );
                f.Open();
                return f;
            }

            public File()
            {
                Content = new List<string>();
            }

            public bool IsOpened
            {
                get { return _isOpened; }
            }

            public void Open()
            {
                Assert.That( _writing == false && _openingOrClosing == false );
                Assert.That( _isOpened == false );
                _openingOrClosing = true;
                if( FileWaitMilliSeconds >= 0 ) Thread.Sleep( FileWaitMilliSeconds );
                _isOpened = true;
                _openingOrClosing = false;
            }

            public void Write( string line )
            {
                Assert.That( _writing == false && _openingOrClosing == false );
                _writing = true;
                Assert.That( _isOpened == true );
                Content.Add( line );
                if( FileWaitMilliSeconds >= 0 ) Thread.Sleep( FileWaitMilliSeconds );
                _writing = false;
            }

            public void Close()
            {
                Assert.That( _writing == false && _openingOrClosing == false );
                Assert.That( _isOpened == true );
                _openingOrClosing = true;
                if( FileWaitMilliSeconds >= 0 ) Thread.Sleep( FileWaitMilliSeconds );
                _isOpened = false;
                _openingOrClosing = false;
            }
        }

        [AttributeUsage( AttributeTargets.Class )]
        class ActionTypeAttribute : Attribute
        {
            public ActionTypeAttribute( Type actionType )
            {
                ActionType = actionType;
            }

            public readonly Type ActionType;

        }

        [ActionType(typeof(WriteIntAction))]
        class WriteActionConfiguration : ActionConfiguration
        {
            public WriteActionConfiguration( string name )
                : base( name )
            {
            }

            public string FileName { get; set; }
        }

        interface ITestIt
        {
            void Initialize( IActivityMonitor m );
            void RunMe( int token );
            void Close( IActivityMonitor m );
        }

        class TestSequence : ITestIt
        {
            readonly string _name;
            readonly ITestIt[] _children;

            internal TestSequence( IActivityMonitor m, ActionSequenceConfiguration c, ITestIt[] children )
            {
                _name = c.Name;
                _children = children;
                m.Info( "Created Sequence '{0}' with {1} children.", _name, _children.Length );
            }

            public void Initialize( IActivityMonitor m )
            {
                m.Info( "Initializing Sequence '{0}'.", _name );
            }

            public void RunMe( int token )
            {
                foreach( var c in _children ) c.RunMe( token );
            }

            public void Close( IActivityMonitor m )
            {
                m.Info( "Closing Sequence '{0}'.", _name );
            }
        }

        class TestParallel : ITestIt
        {
            readonly string _name;
            readonly ITestIt[] _children;

            internal TestParallel( IActivityMonitor m, ActionParallelConfiguration c, ITestIt[] children )
            {
                _name = c.Name;
                _children = children;
                m.Info( "Created Parallel '{0}' with {1} children.", _name, _children.Length );
            }

            public void Initialize( IActivityMonitor m )
            {
                m.Info( "Initializing Parallel '{0}'.", _name );
            }

            public void RunMe( int token )
            {
                IEnumerable<Task> tasks = _children.Select( c => new Task( () => c.RunMe( token ) ) );
                Parallel.ForEach( tasks, t => t.RunSynchronously() );
            }

            public void Close( IActivityMonitor m )
            {
                m.Info( "Closing Parallel '{0}'.", _name );
            }
        }

        class WriteIntAction : ITestIt
        {
            readonly string _name;
            readonly string _fileName;
            File _file;

            public WriteIntAction( IActivityMonitor m,  WriteActionConfiguration config )
            {
                _name = config.Name;
                _fileName = config.FileName;
            }

            public void Initialize( IActivityMonitor m )
            {
                using( m.OpenGroup( LogLevel.Info, "{0} opens file {1}.", _name, _fileName ) )
                {
                    _file = File.OpenOrCreate( _fileName );
                }
            }

            public void RunMe( int token )
            {
                try
                {
                    _file.Write( token.ToString() );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.LoggingError.Add( ex, String.Format( "WriteAction named '{0}'.", _name ) );
                }
            }

           public void Close( IActivityMonitor m )
            {
                _file.Close();
            }
        }

        /// <summary>
        /// FinalRoute contains the root list of actions to execute.
        /// It may route synchronously or not: here, the static bool SynchronousRoute parameters this behavior.
        /// </summary>
        class FinalRoute
        {
            readonly IRouteConfigurationLock _useLock;
            public readonly ITestIt[] Actions;
            readonly string _name;

            public static bool SynchronousRoute = false;

            public static readonly FinalRoute Empty = new FinalRoute( null, Util.EmptyArray<ITestIt>.Empty, String.Empty );

            internal FinalRoute( IRouteConfigurationLock useLock, ITestIt[] actions, string name )
            {
                _useLock = useLock;
                Actions = actions;
                _name = name;
            }

            public void UnlockObtainedRoute()
            {
                _useLock.Unlock();
            }

            public void Run( int token )
            {
                _useLock.Lock();
                if( SynchronousRoute ) HandleRun( token );
                else ThreadPool.QueueUserWorkItem( HandleRun, token );
            }

            void HandleRun( object tokenObject )
            {
                int token = (int)tokenObject;
                try
                {
                    foreach( var a in Actions ) a.RunMe( token );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.LoggingError.Add( ex, "While logging event." );
                }
                finally
                {
                    _useLock.Unlock();
                }
            }
        }

        class TestFactory : RouteActionFactory<ITestIt, FinalRoute>
        {
            protected internal override FinalRoute DoCreateEmptyFinalRoute()
            {
                return FinalRoute.Empty;
            }

            protected override ITestIt DoCreate( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionConfiguration c )
            {
                ActionTypeAttribute a = (ActionTypeAttribute)c.GetType().GetCustomAttributes( typeof( ActionTypeAttribute ), true ).Single();
                return (ITestIt)Activator.CreateInstance( a.ActionType, monitor, c );
            }

            protected override ITestIt DoCreateParallel( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionParallelConfiguration c, ITestIt[] children )
            {
                return new TestParallel( monitor, c, children );
            }

            protected override ITestIt DoCreateSequence( IActivityMonitor monitor, IRouteConfigurationLock configLock, ActionSequenceConfiguration c, ITestIt[] children )
            {
                return new TestSequence( monitor, c, children );
            }

            protected internal override FinalRoute DoCreateFinalRoute( IActivityMonitor monitor, IRouteConfigurationLock configLock, ITestIt[] actions, string configurationName )
            {
                return new FinalRoute( configLock, actions, configurationName );
            }
        }

        ConfiguredRouteHost<ITestIt,FinalRoute> _host;
        FinalRoute _defaultRoute;

        [SetUp]
        public void SetupContext()
        {
            File.AllFiles.Clear();
            ActivityMonitor.LoggingError.Clear();
            File.FileWaitMilliSeconds = -1;
            FinalRoute.SynchronousRoute = true;
            _host = new ConfiguredRouteHost<ITestIt, FinalRoute>( new TestFactory(), OnConfigurationReady, ( m, t ) => t.Initialize( m ), ( m, t ) => t.Close( m ) );
            _host.ConfigurationClosing += _host_ConfigurationClosing;
        }

        void _host_ConfigurationClosing( object sender, ConfiguredRouteHost<ConfigHostTests.ITestIt, ConfigHostTests.FinalRoute>.ConfigurationClosingEventArgs e )
        {
            e.Monitor.Info( "Configuration closing." );
            if( _defaultRoute != null )
            {
                e.Monitor.Info( "Releasing current default route." );
                _defaultRoute.UnlockObtainedRoute();
            }
        }

        void OnConfigurationReady( ConfiguredRouteHost<ITestIt, FinalRoute>.ConfigurationReady ready )
        {
            ready.Monitor.Info( "OnConfigurationReady called." );
        }

        [TearDown]
        public void TearDownContext()
        {
            _host.Dispose();
            File.AllFiles.Clear();
            ActivityMonitor.LoggingError.Clear();
        }

        [Test]
        public void SimpleLogConfigAndDump()
        {
            var c = new RouteConfiguration()
                        .AddAction( new ActionSequenceConfiguration( "Sequence" )
                            .AddAction( new WriteActionConfiguration( "n°1" ) { FileName = @"File n°1" } )
                            .AddAction( new WriteActionConfiguration( "n°2" ) { FileName = @"File n°2" } ) );

            Assert.That( _host.ObtainRoute( "" ), Is.SameAs( FinalRoute.Empty ) );
            
            Assert.That( _host.ConfigurationAttemptCount, Is.EqualTo( 0 ) );
            Assert.That( _host.SetConfiguration( TestHelper.Monitor, c ) );
            Assert.That( _host.SuccessfulConfigurationCount, Is.EqualTo( 1 ) );

            _defaultRoute = _host.ObtainRoute( null );
            Assert.That( _defaultRoute.Actions.Length, Is.EqualTo( 1 ) );
            Assert.That( _defaultRoute.Actions[0], Is.InstanceOf<TestSequence>() );

            var f1 = File.AllFiles["File n°1"];
            var f2 = File.AllFiles["File n°2"];

            Assert.That( f1.IsOpened && f2.IsOpened );
            _defaultRoute.Run( 0 );
            CheckContent( f1, "0" );
            CheckContent( f2, "0" );
            _defaultRoute.Run( 1 );
            _defaultRoute.Run( 2 );
            CheckContent( f1, "0", "1", "2" );
            CheckContent( f2, "0", "1", "2" );

            var c2 = new RouteConfiguration()
                    .AddAction( new WriteActionConfiguration( "n°1" ) { FileName = @"File n°1" } );

            Assert.That( _host.SetConfiguration( TestHelper.Monitor, c2 ) );
            Assert.That( _host.ConfigurationAttemptCount, Is.EqualTo( 2 ) );
            Assert.That( _host.SuccessfulConfigurationCount, Is.EqualTo( 2 ) );

            var r2 = _host.ObtainRoute( null );
            Assert.That( r2, Is.Not.SameAs( _defaultRoute ) );
            _defaultRoute = r2;
            _defaultRoute.Run( 3 );
            _defaultRoute.Run( 4 );
            CheckContent( f1, "0", "1", "2", "3", "4" );
            CheckContent( f2, "0", "1", "2" );

            Assert.That( ActivityMonitor.LoggingError.ToArray().Length, Is.EqualTo( 0 ) );
        }

        private static void CheckContent( File f, params string[] values )
        {
            CollectionAssert.AreEqual( f.Content, values, StringComparer.Ordinal, "Incorrect values." );
        }

    }
}
