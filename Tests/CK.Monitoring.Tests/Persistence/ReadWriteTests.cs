#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Persistence\ReadWriteTests.cs) is part of CiviKey. 
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
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests.Persistence
{
    [TestFixture]
    public class ReadWriteTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Test]
        public void LogEntryReadWrite()
        {
            var exInner = new CKExceptionData( "message", "typeof(exception)", "assemblyQualifiedName", "stackTrace", null, "fileName", "fusionLog", null, null );
            var ex2 = new CKExceptionData( "message2", "typeof(exception2)", "assemblyQualifiedName2", "stackTrace2", exInner, "fileName2", "fusionLog2", null, null );
            var exL = new CKExceptionData( "loader-message", "typeof(loader-exception)", "loader-assemblyQualifiedName", "loader-stackTrace", null, "loader-fileName", "loader-fusionLog", null, null );
            var exAgg = new CKExceptionData( "agg-message", "typeof(agg-exception)", "agg-assemblyQualifiedName", "agg-stackTrace", ex2, "fileName", "fusionLog", null, new[]{ ex2, exL } );

            var prevLog = DateTimeStamp.UtcNow;
            ILogEntry e1 = LogEntry.CreateLog( "Text1", new DateTimeStamp( DateTime.UtcNow, 42 ), LogLevel.Info, "c:\\test.cs", 3712, ActivityMonitor.Tags.CreateDependentActivity, exAgg );
            ILogEntry e2 = LogEntry.CreateMulticastLog( Guid.Empty, LogEntryType.Line, prevLog, 5, "Text2", DateTimeStamp.UtcNow, LogLevel.Fatal, null, 3712, ActivityMonitor.Tags.CreateDependentActivity, exAgg );

            using( var mem = new MemoryStream() )
            using( var w = new BinaryWriter( mem ) )
            {
                w.Write( LogReader.CurrentStreamVersion );
                e1.WriteLogEntry( w );
                e2.WriteLogEntry( w );
                w.Write( (byte)0 );

                mem.Position = 0;
                using( var reader = new LogReader( mem ) )
                {
                    Assert.That( reader.MoveNext() );
                    Assert.That( reader.Current.Text, Is.EqualTo( e1.Text ) );
                    Assert.That( reader.Current.LogLevel, Is.EqualTo( e1.LogLevel ) );
                    Assert.That( reader.Current.LogTime, Is.EqualTo( e1.LogTime ) );
                    Assert.That( reader.Current.FileName, Is.EqualTo( e1.FileName ) );
                    Assert.That( reader.Current.LineNumber, Is.EqualTo( e1.LineNumber ) );
                    Assert.That( reader.Current.Exception.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( e1.Exception.ExceptionTypeAssemblyQualifiedName ) );
                    Assert.That( reader.Current.Exception.ToString(), Is.EqualTo( e1.Exception.ToString() ) );

                    Assert.That( reader.MoveNext() );
                    Assert.That( reader.CurrentMulticast.PreviousEntryType, Is.EqualTo( LogEntryType.Line ) );
                    Assert.That( reader.CurrentMulticast.PreviousLogTime, Is.EqualTo( prevLog ) );
                    Assert.That( reader.Current.Text, Is.EqualTo( e2.Text ) );
                    Assert.That( reader.Current.LogTime, Is.EqualTo( e2.LogTime ) );
                    Assert.That( reader.Current.FileName, Is.Null );
                    Assert.That( reader.Current.LineNumber, Is.EqualTo( 0 ), "Since no file name is set, line number is 0." );
                    Assert.That( reader.Current.Exception.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( e2.Exception.ExceptionTypeAssemblyQualifiedName ) );
                    Assert.That( reader.Current.Exception.ToString(), Is.EqualTo( e2.Exception.ToString() ) );
                    
                    Assert.That( reader.MoveNext(), Is.False );
                    Assert.That( reader.BadEndOfFileMarker, Is.False );
                }
            }

        }


        [Test]
        public void CKMonWriterClientTest()
        {
            TestHelper.CleanupTestFolder();
            using( TextWriter w = new StringWriter() )
            {
                var m = new ActivityMonitor();
                m.Output.RegisterClient( new ActivityMonitorTextWriterClient( s => w.Write( s ) ) );
                m.Output.RegisterClient( new ActivityMonitorErrorCounter( true ) );

                using( m.OpenWarn().Send( "First" ) )
                {
                    m.Info().Send( "Inner first test" );
                }

                w.Flush();
                Assert.That( w.ToString(), Is.Not.Empty );
                Assert.That( w.ToString(), Is.StringContaining( "First" ) );
                Assert.That( w.ToString(), Is.StringContaining( "Inner first test" ) );
                Assert.That( w.ToString(), Is.Not.StringContaining( "Second" ) );
                Assert.That( w.ToString(), Is.Not.StringContaining( "Inner second test" ) );


                m.Output.RegisterClient( new CKMonWriterClient( Path.Combine( TestHelper.TestFolder, "CKMonWriterClientTest" ), 20000, LogFilter.Undefined ) );

                using( m.OpenWarn().Send( "Second" ) )
                {
                    m.Info().Send( "Inner second test" );
                }

                w.Flush();

                Assert.That( w.ToString(), Is.Not.Empty );
                Assert.That( w.ToString(), Is.StringContaining( "First" ) );
                Assert.That( w.ToString(), Is.StringContaining( "Inner first test" ) );
                Assert.That( w.ToString(), Is.StringContaining( "Second" ) );
                Assert.That( w.ToString(), Is.StringContaining( "Inner second test" ) );

            }
        }
    }
}
