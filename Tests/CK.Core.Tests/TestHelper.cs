using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
{
    static partial class TestHelper
    {
        static string _testFolder;
        static string _solutionFolder;
        
        static TestHelper()
        {
        }

#if DNX451 || DNX46
        /// <summary>
        /// Use reflection to actually set <see cref="System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> to 5 milliseconds.
        /// This triggers an immediate polling from the internal .Net framework LeaseManager.
        /// Note that the LeaseManager is per AppDomain.
        /// </summary>
        public static void SetRemotingLeaseManagerVeryShortPollTime()
        {
            System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime = TimeSpan.FromMilliseconds( 5 );
            object remotingData = typeof( AppDomain ).GetProperty( "RemotingData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod( true ).Invoke( System.Threading.Thread.GetDomain(), null );
            if( remotingData != null )
            {
                object leaseManager = remotingData.GetType().GetProperty( "LeaseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod( true ).Invoke( remotingData, null );
                if( leaseManager != null )
                {
                    System.Threading.Timer timer = (System.Threading.Timer)leaseManager.GetType().GetField( "leaseTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue( leaseManager );
                    Assert.That( timer, Is.Not.Null );
                    timer.Change( 0, -1 );
                }
            }
        }
#endif

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitalizePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            DeleteFolder( TestFolder, true );
        }

        static public void ForceGCFullCollect()
        {
            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced, true );
        }

        public static void DeleteFolder( string directoryPath, bool recreate = false )
        {
            int tryCount = 0;
            for( ; ; )
            {
                try
                {
                    if( Directory.Exists( directoryPath ) ) Directory.Delete( directoryPath, true );
                    if( recreate )
                    {
                        Directory.CreateDirectory( directoryPath );
                        File.WriteAllText( Path.Combine( directoryPath, "TestWrite.txt" ), "Test write works." );
                        File.Delete( Path.Combine( directoryPath, "TestWrite.txt" ) );
                    }
                    return;
                }
                catch( Exception ex )
                {
                    if( ++tryCount == 20 ) throw;
                    Console.WriteLine( "{1} - While cleaning up directory '{0}'. Retrying.", directoryPath, ex.Message );
                    System.Threading.Thread.Sleep( 100 );
                }
            }
        }

        private static void InitalizePaths()
        {
#if DNX451 || DNX46
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            // => CK.XXX.Tests/bin/Debug/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/bin/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/
            p = Path.GetDirectoryName( p );
            // ==> CK.XXX.Tests/TestDir
            _testFolder = Path.Combine( p, "TestDir" );
            do
            {
                p = Path.GetDirectoryName( p );
            }
            while( !File.Exists( Path.Combine( p, "CK-Core.sln" ) ) );
            _solutionFolder = p;

            Console.WriteLine( "SolutionFolder is: {1}\r\nTestFolder is: {0}", _testFolder, _solutionFolder );
            CleanupTestFolder();
#endif
        }
    }
}
