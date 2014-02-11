#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\SimpleSampleTests.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests.Monitoring
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "ActivityMonitor" )]
    public class DocumentationCodeSnippets
    {
        [Test]
        public void SimpleUsage()
        {
            var f = new FileInfo( Path.Combine( TestHelper.SolutionFolder, @"Tests\CK.Core.Tests\Animals.cs" ) );
            DemoLogs( TestHelper.ConsoleMonitor, f, new Exception() );
            DemoOpenGroupFarFromPerfect( TestHelper.ConsoleMonitor );
            DemoOpenGroupBetter( TestHelper.ConsoleMonitor );
            DemoOpenGroupThisWorksFine( TestHelper.ConsoleMonitor );
            DemoOpenGroupWithDynamicConclusion( TestHelper.ConsoleMonitor );
            DoSomething( TestHelper.ConsoleMonitor, f );
        }

        void DemoOpenGroupFarFromPerfect( IActivityMonitor m )
        {
            m.OpenInfo().Send( "Doing things..." );
            // ...
            m.CloseGroup( "Success." );
        }

        void DemoOpenGroupBetter( IActivityMonitor m )
        {
            using( m.OpenInfo().Send( "Doing things..." ) )
            {
                // ...
            }
        }

        void DemoOpenGroupThisWorksFine( IActivityMonitor m )
        {
            using( m.OpenInfo().Send( "Doing things..." ) )
            {
                // ...
                m.CloseGroup( "Success." );
            }
        }

        void DemoOpenGroupWithDynamicConclusion( IActivityMonitor m )
        {
            int nbProcessed = 0;
            using( m.OpenInfo().Send( "Doing things..." )
                                .ConcludeWith( () => String.Format( "{0} files.", nbProcessed ) ) )
            {
                // ...
                nbProcessed += 21;
                m.CloseGroup( "Success." );
                // The user Group conclusion is: "Success. - 21 files." (the two conclusions are concatenated).
            }
        }

        void DemoLogs( IActivityMonitor m, FileInfo f, Exception ex )
        {
            m.Trace().Send( "Data from '{0}' processed.", f.Name );
            m.Info().Send( ex, "An error occurred while processing '{0}'. Process will be retried later.", f.Name );
            m.Warn().Send( "File '{0}' is too big ({1} Kb). It must be less than 50Kb.", f.Name, f.Length / 1024 );
            m.Error().Send( ex, "File '{0}' can not be processed.", f.Name );
            m.Fatal().Send( ex, "This will cancel the whole operation." );
        }

        void Create()
        {
            {
                var m = new ActivityMonitor();
            }
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
            }
            {
                IActivityMonitor m = new ActivityMonitor();
                var counter = new ActivityMonitorErrorCounter();
                m.Output.RegisterClient( counter );

                m.Fatal().Send( "An horrible error occurred." );

                Assert.That( counter.Current.FatalCount == 1 );
                m.Output.UnregisterClient( counter );
            }
            {
                IActivityMonitor m = new ActivityMonitor();

                int errorCount = 0;
                using( m.CatchCounter( fatalOrErrorCount => errorCount = fatalOrErrorCount ) )
                {
                    m.Fatal().Send( "An horrible error occurred." );
                }
                Assert.That( errorCount == 1 );
            }
            {
                IActivityMonitor m = new ActivityMonitor();
                m.MinimalFilter = LogFilter.Off;
                // ...
                m.MinimalFilter = LogFilter.Debug;
            }
            {
                IActivityMonitor m = new ActivityMonitor();
                m.MinimalFilter = LogFilter.Off;
                // ...
                using( m.SetMinimalFilter( LogFilter.Debug ) )
                {
                    Assert.That( m.ActualFilter == LogFilter.Debug );
                    // ...
                }
                Assert.That( m.ActualFilter == LogFilter.Off, "Filter has been restored to previous value." );
            }
            {
                IActivityMonitor m = new ActivityMonitor();
                m.MinimalFilter = LogFilter.Off;
                // ...
                using( m.OpenWarn().Send( "Ouch..." ) )
                {
                    Assert.That( m.ActualFilter == LogFilter.Off );
                    m.MinimalFilter = LogFilter.Debug;
                    // ... in debug filter ...
                }
                Assert.That( m.ActualFilter == LogFilter.Off, "Back to Off." );

                var strange = new LogFilter( LogLevelFilter.Fatal, LogLevelFilter.Trace );
            }
        }

        bool DoSomething( IActivityMonitor m, FileInfo file )
        {
            using( m.OpenInfo().Send( "Do something important on file '{0}'.", file.Name ) )
            {
                if( !file.Exists )
                {
                    m.Warn().Send( "File does not exist." );
                }
                else
                {
                    m.Trace().Send( "File last modified at {1:T}. {0} Kb to process.", file.Length, file.LastWriteTimeUtc );
                    try
                    {
                        // ... Process file ...
                    }
                    catch( Exception ex )
                    {
                        m.Error().Send( ex, "While processing." );
                        return false;
                    }
                }
                m.SetTopic( "Changing my mind. Keeping it as-is." );
                return true;
            }
        }

    }
}
