#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\SourceFilteringTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CK.Core.Tests.Monitoring
{
    public class SourceFilteringTests
    {
        [Fact]
        public void we_use_the_fact_that_FileNames_are_interned_strings()
        {
            ThisFile();
        }

        string ThisFile( [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            #if NET452 || NET46
             String.IsInterned( fileName ).Should().NotBeNull();
            #endif
             lineNumber.Should().BeGreaterThan( 0 );
            return fileName;
        }

        [Fact]
        public void ActivityMonitor_SourceFilter_handles_overrides_of_filter_per_source_file()
        {
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                 m.ActualFilter.Should().Be( LogFilter.Undefined  );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Release );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Undefined );
                m.Trace().Send( "Trace2" );
                m.OpenTrace().Send( "OTrace2" );

                c.Entries.Select(e => e.Text).ToArray().ShouldBeEquivalentTo(new[] { "Trace1", "OTrace1", "Trace2", "OTrace2" }, o => o.WithStrictOrdering() );
            }
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                m.MinimalFilter = LogFilter.Terse;
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Trace );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetOverrideFilter( LogFilter.Undefined );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );

                c.Entries.Select(e => e.Text).ToArray().ShouldBeEquivalentTo(new[] { "Trace1", "OTrace1" }, o => o.WithStrictOrdering());
            }
        }

        [Fact]
        public void ActivityMonitor_SourceFilter_handles_minimal_filter_setting_per_source_file()
        {
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                 m.ActualFilter.Should().Be( LogFilter.Undefined );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Release );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Undefined );
                m.Trace().Send( "Trace2" );
                m.OpenTrace().Send( "OTrace2" );

                c.Entries.Select(e => e.Text).ToArray().ShouldBeEquivalentTo(new[] { "Trace1", "OTrace1", "Trace2", "OTrace2" }, o => o.WithStrictOrdering());
            }
            {
                var m = new ActivityMonitor( applyAutoConfigurations: false );
                var c = m.Output.RegisterClient( new StupidStringClient() );

                m.MinimalFilter = LogFilter.Terse;
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Trace );
                m.Trace().Send( "Trace1" );
                m.OpenTrace().Send( "OTrace1" );
                ActivityMonitor.SourceFilter.SetMinimalFilter( LogFilter.Undefined );
                m.Trace().Send( "NOSHOW" );
                m.OpenTrace().Send( "NOSHOW" );

                c.Entries.Select(e => e.Text).ToArray().ShouldBeEquivalentTo(new[] { "Trace1", "OTrace1" }, o => o.WithStrictOrdering());;
            }
        }

    }
}
