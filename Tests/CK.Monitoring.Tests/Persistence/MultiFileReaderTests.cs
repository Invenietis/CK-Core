#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Persistence\MultiFileReaderTests.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests.Persistence
{
    [TestFixture]
    public class MultiFileReaderTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Test]
        public void ReadDuplicates()
        {
            Stopwatch sw = new Stopwatch();
            for( int nbEntries1 = 1; nbEntries1 < 8; ++nbEntries1 )
                for( int nbEntries2 = 1; nbEntries2 < 8; ++nbEntries2 )
                {
                    TestHelper.ConsoleMonitor.Trace().Send( "Start {0}/{1}.", nbEntries1, nbEntries2 );
                    sw.Restart();
                    DuplicateTestWith6Entries( nbEntries1, nbEntries2 );
                    TestHelper.ConsoleMonitor.Trace().Send( "Done in {0}.", sw.Elapsed );
                }
        }

        [Test]
        public void ReadDuplicatesWithGZip()
        {
            Stopwatch sw = new Stopwatch();
            for( int nbEntries1 = 1; nbEntries1 < 8; ++nbEntries1 )
                for( int nbEntries2 = 1; nbEntries2 < 8; ++nbEntries2 )
                {
                    TestHelper.ConsoleMonitor.Trace().Send( "Start {0}/{1}.", nbEntries1, nbEntries2 );
                    sw.Restart();
                    DuplicateTestWith6Entries( nbEntries1, nbEntries2, true );
                    TestHelper.ConsoleMonitor.Trace().Send( "Done in {0}.", sw.Elapsed );
                }
        }

        private static void DuplicateTestWith6Entries( int nbEntries1, int nbEntries2, bool gzip = false )
        {
            var folder = String.Format( "{0}\\ReadDuplicates", TestHelper.TestFolder );
            TestHelper.CleanupFolder( folder );
            string config = String.Format( @"
<GrandOutputConfiguration>
    <Channel>
        <Add Type=""BinaryFile"" Name=""All-1"" MaxCountPerFile=""{1}"" Path=""{0}"" UseGzipCompression=""{3}"" /> 
        <Add Type=""BinaryFile"" Name=""All-2"" MaxCountPerFile=""{2}"" Path=""{0}"" UseGzipCompression=""{3}"" /> 
    </Channel>
</GrandOutputConfiguration>
", folder, nbEntries1, nbEntries2, gzip );

            using( var o = new GrandOutput() )
            {
                GrandOutputConfiguration c = new GrandOutputConfiguration();
                Assert.That( c.Load( XDocument.Parse( config ).Root, TestHelper.ConsoleMonitor ), Is.True );
                Assert.That( o.SetConfiguration( c, TestHelper.ConsoleMonitor ), Is.True );

                var m = new ActivityMonitor();
                o.Register( m );
                var direct = m.Output.RegisterClient( new CKMonWriterClient( folder, Math.Min( nbEntries1, nbEntries2 ), LogFilter.Debug, gzip ) );
                // 6 traces that go to the GrandOutput but also to the direct CKMonWriterClient.
                m.Trace().Send( "Trace 1" );
                m.OpenTrace().Send( "OpenTrace 1" );
                m.Trace().Send( "Trace 1.1" );
                m.Trace().Send( "Trace 1.2" );
                m.CloseGroup();
                m.Trace().Send( "Trace 2" );
                m.Output.UnregisterClient( direct );
            }
            var files = TestHelper.WaitForCkmonFilesInDirectory( folder, 3 );
            for( int pageReadLength = 1; pageReadLength < 10; ++pageReadLength )
            {
                MultiLogReader reader = new MultiLogReader();
                reader.Add( files );
                var map = reader.GetActivityMap();
                Assert.That( map.ValidFiles.All( rawFile => rawFile.IsValidFile ), Is.True, "All files are correctly closed with the final 0 byte and no exception occurred while reading them." );

                var readMonitor = map.Monitors.Single();

                List<ParentedLogEntry> allEntries = new List<ParentedLogEntry>();
                using( var pageReader = readMonitor.ReadFirstPage( pageReadLength ) )
                {
                    do
                    {
                        allEntries.AddRange( pageReader.Entries );
                    }
                    while( pageReader.ForwardPage() > 0 );
                }
                CollectionAssert.AreEqual( new[] { "Trace 1", "OpenTrace 1", "Trace 1.1", "Trace 1.2", null, "Trace 2" }, allEntries.Select( e => e.Entry.Text ).ToArray(), StringComparer.Ordinal );
            }
        }

    }
}
