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
        [Test, Explicit]
        public async void GzipSyncAsyncTests()
        {
            int[] fileSizes = new int[] { 10000000, 20000000, 50000000 };
            int[] bufferSizes = new int[] { 4000, 8000, 16000, 32000 };

            byte[] fileBuffer;
            Random r = new Random();

            Stopwatch sw = new Stopwatch();

            // Write to file
            using( var tf = new TemporaryFile( true, "bin" ) )
            {
                using( var tfOut = new TemporaryFile( true, "bin.gz" ) )
                {
                    foreach( int fileSize in fileSizes )
                    {
                        fileBuffer = new byte[fileSize];
                        r.NextBytes( fileBuffer );
                        using( var f = File.OpenWrite( tf.Path ) ) { await f.WriteAsync( fileBuffer, 0, fileBuffer.Length ); }

                        foreach( int bufferSize in bufferSizes )
                        {
                            sw.Start();
                            FileUtil.CompressFileToGzipFile( tf.Path, tfOut.Path, false, bufferSize );
                            sw.Stop();

                            TestHelper.ConsoleMonitor.Info().Send( "Synchronous Gzip write: {0} ms / File size: {1} bytes / Buffer: {2} bytes", sw.ElapsedMilliseconds, fileSize, bufferSize );

                            sw.Reset();
                            sw.Start();
                            await FileUtil.CompressFileToGzipFileAsync( tf.Path, tfOut.Path, CancellationToken.None, false, bufferSize );
                            sw.Stop();

                            TestHelper.ConsoleMonitor.Info().Send( "Asynchronous Gzip write: {0} ms / File size: {1} bytes / Buffer: {2} bytes", sw.ElapsedMilliseconds, fileSize, bufferSize );
                        }
                    }
                }
            }
        }
    }
}
