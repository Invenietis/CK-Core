#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Performance\GzipFileTests.cs) is part of CiviKey. 
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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using NUnit.Framework;
namespace CK.Monitoring.Tests.Performance
{
    [TestFixture]
    public class GzipFileTests
    {
#if net45
        [Test, Explicit]
        public async void GzipSyncAsyncPerformanceTests()
        {
            int[] fileSizes = new int[] { 5 * 1024 * 1024, 10 * 1024 * 1024, 30 * 1024 * 1024 };
            int[] bufferSizes = new int[] { 1 * 1024, 2 * 1024, 4 * 1024, 32 * 1024, 64 * 1024 };

            byte[] fileBuffer = new byte[fileSizes.Max()];
            new Random().NextBytes( fileBuffer );

            Stopwatch sw = new Stopwatch();

            // Write to file
            using( var tf = new TemporaryFile( true, "bin" ) )
            {
                using( var tfOut = new TemporaryFile( true, "bin.gz" ) )
                {
                    foreach( int fileSize in fileSizes )
                    {
                        using( var f = File.OpenWrite( tf.Path ) ) { await f.WriteAsync( fileBuffer, 0, fileSize ); }
                        Thread.Sleep( 10 );
                        using( TestHelper.ConsoleMonitor.OpenInfo().Send( "File Size = {0} MiB", fileSize / 1024 / 1024 ) )
                        {
                            foreach( int bufferSize in bufferSizes )
                            {
                                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Buffer Size = {0} bytes", bufferSize ) )
                                {
                                    sw.Start();
                                    FileUtil.CompressFileToGzipFile( tf.Path, tfOut.Path, false, CompressionLevel.Optimal, bufferSize );
                                    sw.Stop();
                                    TestHelper.ConsoleMonitor.Info().Send( "-Synchronous Gzip write: {0:######} ms.", sw.ElapsedMilliseconds );
                                    sw.Reset();
                                    sw.Start();
                                    await FileUtil.CompressFileToGzipFileAsync( tf.Path, tfOut.Path, CancellationToken.None, false, CompressionLevel.Optimal, bufferSize );
                                    sw.Stop();
                                    TestHelper.ConsoleMonitor.Info().Send( "Asynchronous Gzip write: {0:######} ms.", sw.ElapsedMilliseconds );
                                    sw.Reset();
                                }
                            }
                        }
                    }
                }
            }
        }
#endif

        [Test, Timeout( 30000 )]
        public void CompressedReadWriteTests()
        {
            TestHelper.CleanupTestFolder();
            string directoryPath = Path.Combine( TestHelper.TestFolder, "GzipCKMonWriterClientTest" );
            ActivityMonitor m = new ActivityMonitor();
            var client = new CKMonWriterClient( directoryPath, 20000, LogFilter.Undefined, true );
            m.Output.RegisterClient( client );
            using( m.OpenWarn().Send( "Group test" ) )
            {
                m.Info().Send( "Line test" );
            }
            // This closes the client: the file is then compressed asynchronously
            // on a thread from the ThreadPool.
            Assert.That( client.IsOpened );
            m.Output.UnregisterClient( client );
            string ckmonPath = TestHelper.WaitForCkmonFilesInDirectory( directoryPath, 1 )[0];
            LogReader r = LogReader.Open( ckmonPath );
            r.MoveNext();
            Assert.That( r.Current.LogType, Is.EqualTo( LogEntryType.OpenGroup ) );
            Assert.That( r.Current.Text, Is.EqualTo( "Group test" ) );
            r.MoveNext();
            Assert.That( r.Current.LogType, Is.EqualTo( LogEntryType.Line ) );
            Assert.That( r.Current.Text, Is.EqualTo( "Line test" ) );
            r.MoveNext();
            Assert.That( r.Current.LogType, Is.EqualTo( LogEntryType.CloseGroup ) );
            bool hasRemainingEntries = r.MoveNext();
            Assert.That( hasRemainingEntries, Is.False );
        }

    }
}