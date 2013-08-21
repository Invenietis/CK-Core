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

        class File
        {
            bool _isOpened;
            List<string> _content;

            public File()
            {
                _content = new List<string>();
            }

            public bool IsOpened
            {
                get { return _isOpened; }
            }

            public void Open()
            {
                Assert.That( _isOpened == false );
                _isOpened = true;
            }

            public void Write( string line )
            {
                Assert.That( _isOpened == true );
                _content.Add( line );
            }

            public void Close()
            {
                Assert.That( _isOpened == true );
                _isOpened = false;
            }
        }

        class FileManager
        {
            Dictionary<string,File> _files = new Dictionary<string, File>();
            
            public File OpenOrCreate( string fileName )
            {
                File f = _files.GetValueWithDefaultFunc( fileName, n => new File() );
                f.Open();
                return f;
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

        [ActionType(typeof(WriteAction))]
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
            void Initialize( IActivityMonitor m, FileManager manager );
            void RunMe( int token );
            void Close( IActivityMonitor m, FileManager manager );
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

            public void Initialize( IActivityMonitor m, FileManager manager )
            {
                m.Info( "Initializing Sequence '{0}'.", _name );
            }

            public void RunMe( int token )
            {
                foreach( var c in _children ) c.RunMe( token );
            }

            public void Close( IActivityMonitor m, FileManager manager )
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

            public void Initialize( IActivityMonitor m, FileManager manager )
            {
                m.Info( "Initializing Parallel '{0}'.", _name );
            }

            public void RunMe( int token )
            {
                IEnumerable<Task> tasks = _children.Select( c => new Task( () => c.RunMe( token ) ) );
                Parallel.ForEach( tasks, t => t.RunSynchronously() );
            }

            public void Close( IActivityMonitor m, FileManager manager )
            {
                m.Info( "Closing Sequence '{0}'.", _name );
            }
        }

        class WriteAction : ITestIt
        {
            readonly string _name;
            readonly string _fileName;
            File _file;

            public WriteAction( IActivityMonitor m,  WriteActionConfiguration config )
            {
                _name = config.Name;
                _fileName = config.FileName;
            }

            public void Initialize( IActivityMonitor m, FileManager manager )
            {
                using( m.OpenGroup( LogLevel.Info, "{0} opens file {1}.", _name, _fileName ) )
                {
                    _file = manager.OpenOrCreate( _fileName );
                }
            }

            public void RunMe( int token )
            {
                try
                {
                    _file.Write( String.Format( "{0},", token ) );
                }
                catch( Exception ex )
                {
                    ActivityMonitor.LoggingError.Add( ex, String.Format( "WriteAction named '{0}'.", _name ) );
                }
            }

           public void Close( IActivityMonitor m, FileManager manager )
            {
                _file.Close();
            }
        }

        class FinalRoute
        {
            readonly IRouteConfigurationLock _useLock;
            public readonly ITestIt[] Actions;
            readonly string _name;

            public static readonly FinalRoute Empty = new FinalRoute( null, Util.EmptyArray<ITestIt>.Empty, String.Empty );

            internal FinalRoute( IRouteConfigurationLock useLock, ITestIt[] actions, string name )
            {
                _useLock = useLock;
                Actions = actions;
                _name = name;
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

        [Test]
        public void CreateConfigHost()
        {
            var c = new RouteConfiguration()
                        .AddAction( new ActionSequenceConfiguration( "Sequence" )
                            .AddAction( new WriteActionConfiguration( "n°1" ) { FileName = @"C:\Test.tst" } )
                            .AddAction( new WriteActionConfiguration( "n°2" ) { FileName = @"C:\Test\" } ) );
            FileManager manager = new FileManager();
            var host = new ConfiguredRouteHost<ITestIt,FinalRoute>( new TestFactory(), OnConfigurationReady, ( m, t ) => t.Initialize( m, manager ), ( m, t ) => t.Close( m, manager ) );

            Assert.That( host.ObtainRoute( "" ), Is.SameAs( FinalRoute.Empty ) );
            
            Assert.That( host.ConfigurationAttemptCount, Is.EqualTo( 0 ) );
            Assert.That( host.SetConfiguration( TestHelper.Monitor, c ) );
            Assert.That( host.SuccessfulConfigurationCount, Is.EqualTo( 1 ) );

            var r = host.ObtainRoute( "" );
            Assert.That( r.Actions.Length, Is.EqualTo( 1 ) );


        }

        void OnConfigurationReady( ConfiguredRouteHost<ITestIt, FinalRoute>.ConfigurationReady ready )
        {

        }
    }
}
