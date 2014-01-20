using System;
using System.Collections.Generic;
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
        [Explicit( "This test is far too expensive because of GrandOutput disposing. This is to investigate." )]
        public void ReadDuplicates()
        {
            for( int nbEntries1 = 1; nbEntries1 < 8; ++nbEntries1 )
                for( int nbEntries2 = 1; nbEntries2 < 8; ++nbEntries2 )
                    for( int pageReadLength = 1; pageReadLength < 8; ++pageReadLength )
                    {
                        DuplicateTestWith6Entries( nbEntries1, nbEntries2, pageReadLength );
                    }
        }

        private static void DuplicateTestWith6Entries( int nbEntries1, int nbEntries2, int pageReadLength )
        {
            var folder = String.Format( "{0}\\ReadDuplicates", TestHelper.TestFolder );
            TestHelper.CleanupFolder( folder );
            string config = String.Format( @"
<GrandOutputConfiguration>
    <Channel>
        <Add Type=""BinaryFile"" Name=""All-1"" MaxCountPerFile=""{1}"" Path=""{0}"" /> 
        <Add Type=""BinaryFile"" Name=""All-2"" MaxCountPerFile=""{2}"" Path=""{0}"" /> 
    </Channel>
</GrandOutputConfiguration>
", folder, nbEntries1, nbEntries2 );

            using( var o = new GrandOutput() )
            {
                GrandOutputConfiguration c = new GrandOutputConfiguration();
                Assert.That( c.Load( XDocument.Parse( config ).Root, TestHelper.ConsoleMonitor ), Is.True );
                Assert.That( o.SetConfiguration( c, TestHelper.ConsoleMonitor ), Is.True );

                var m = new ActivityMonitor();
                o.Register( m );

                // 6 traces.
                m.Trace().Send( "Trace 1" );
                m.OpenTrace().Send( "OpenTrace 1" );
                m.Trace().Send( "Trace 1.1" );
                m.Trace().Send( "Trace 1.2" );
                m.CloseGroup();
                m.Trace().Send( "Trace 2" );
            }

            MultiLogReader reader = new MultiLogReader();
            reader.Add( Directory.EnumerateFiles( folder ) );
            var map = reader.GetActivityMap();
            Assert.That( map.ValidFiles.All( rawFile => rawFile.IsValdFile ), Is.True, "All files are correctly closed with the final 0 byte and no exception occurred while reading them." );

            var readMonitor = map.Monitors.Single();

            List<ParentedLogEntry> allEntries = new List<ParentedLogEntry>();
            var pageReader = readMonitor.ReadFirstPage( pageReadLength );
            do
            {
                allEntries.AddRange( pageReader.Entries );
            }
            while( pageReader.ForwardPage() > 0 );

            CollectionAssert.AreEqual( allEntries.Select( e => e.Entry.Text ), new[] { "Trace 1", "OpenTrace 1", "Trace 1.1", "Trace 1.2", null, "Trace 2" }, StringComparer.Ordinal );
        }

    }
}
