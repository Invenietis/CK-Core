#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\TestHelper.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests
{
    [ExcludeFromCodeCoverage]
    static class TestHelper
    {
        static string _testFolder;
        static string _solutionFolder;
        
        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            _console = _monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
        }

        public static IActivityMonitor ConsoleMonitor
        {
            get { return _monitor; }
        }

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value ) _monitor.Output.RegisterUniqueClient( c => c == _console, () => _console );
                else _monitor.Output.UnregisterClient( _console );
            }
        }

        /// <summary>
        /// Use reflection to actually set <see cref="System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> to 5 milliseconds.
        /// This triggers an immediate polling from the internal .Net framework LeaseManager.
        /// Note that the LeaseManager is per AppDomain.
        /// </summary>
        public static void SetRemotingLeaseManagerVeryShortPollTime()
        {
            System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime = TimeSpan.FromMilliseconds( 5 );
            object remotingData = typeof( AppDomain ).GetProperty( "RemotingData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod().Invoke( System.Threading.Thread.GetDomain(), null );
            if( remotingData != null )
            {
                object leaseManager = remotingData.GetType().GetProperty( "LeaseManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetGetMethod().Invoke( remotingData, null );
                if( leaseManager != null )
                {
                    System.Threading.Timer timer = (System.Threading.Timer)leaseManager.GetType().GetField( "leaseTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ).GetValue( leaseManager );
                    Assert.That( timer, Is.Not.Null );
                    timer.Change( 0, -1 );
                }
            }
        }

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
            CleanupFolder( TestFolder );
        }

        public static void CleanupFolder( string folder )
        {
            int tryCount = 0;
            for( ; ; )
            {
                try
                {
                    if( Directory.Exists( folder ) ) Directory.Delete( folder, true );
                    Directory.CreateDirectory( folder );
                    File.WriteAllText( Path.Combine( folder, "TestWrite.txt" ), "Test write works." );
                    File.Delete( Path.Combine( folder, "TestWrite.txt" ) );
                    return;
                }
                catch( Exception ex )
                {
                    if( ++tryCount == 20 ) throw;
                    ConsoleMonitor.Info().Send( ex, "While cleaning up test directory. Retrying." );
                    System.Threading.Thread.Sleep( 100 );
                }
            }
        }

        public static void InitalizePaths()
        {
            if( _testFolder != null ) return;
            var p = _testFolder = GetTestFolder();
            do
            {
                p = Path.GetDirectoryName( p );
            }
            while( !File.Exists( Path.Combine( p, "CK-Core.sln" ) ) );
            _solutionFolder = p;

            SystemActivityMonitor.RootLogPath = Path.Combine( TestHelper.TestFolder, "RootLogPath" );
            ConsoleMonitor.Info().Send( "SolutionFolder is: {1}\r\nTestFolder is: {0}\r\nRootLogPath is: {2}", _testFolder, _solutionFolder, SystemActivityMonitor.RootLogPath );

            CleanupTestFolder();
        }

        /// <summary>
        /// Can be called from another application domain (does not set SystemActivityMonitor.RootLogPath not initialize statics).
        /// </summary>
        /// <returns>The /TestFolder for this project.</returns>
        public static string GetTestFolder()
        {
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            // => CK.XXX.Tests/bin/Debug/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/bin/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/
            p = Path.GetDirectoryName( p );
            // ==> CK.XXX.Tests/TestDir
            return Path.Combine( p, "TestFolder" );
        }

    }
}
