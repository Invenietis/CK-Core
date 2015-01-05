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
        public async void GzipSyncAsyncPerformanceTests()
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
#if net40
                        using( var f = File.OpenWrite( tf.Path ) ) { f.Write( fileBuffer, 0, fileBuffer.Length ); }
#else
                        using( var f = File.OpenWrite( tf.Path ) ) { await f.WriteAsync( fileBuffer, 0, fileBuffer.Length ); }
#endif

                        foreach( int bufferSize in bufferSizes )
                        {
                            sw.Start();
                            FileUtil.CompressFileToGzipFile( tf.Path, tfOut.Path, false, bufferSize );
                            sw.Stop();

                            TestHelper.ConsoleMonitor.Info().Send( "Synchronous Gzip write: {0} ms / File size: {1} bytes / Buffer: {2} bytes", sw.ElapsedMilliseconds, fileSize, bufferSize );

                            sw.Reset();
                            sw.Start();
#if net40
                            FileUtil.CompressFileToGzipFile( tf.Path, tfOut.Path, false, bufferSize );
#else
                            await FileUtil.CompressFileToGzipFileAsync( tf.Path, tfOut.Path, CancellationToken.None, false, bufferSize );
#endif
                            sw.Stop();

                            TestHelper.ConsoleMonitor.Info().Send( "Asynchronous Gzip write: {0} ms / File size: {1} bytes / Buffer: {2} bytes", sw.ElapsedMilliseconds, fileSize, bufferSize );
                        }
                    }
                }
            }
        }

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

            m.Output.UnregisterClient( client );
            client.Close();

            string ckmonPath = WaitForCkmonFileInDirectory( directoryPath );

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

        string WaitForCkmonFileInDirectory( string directoryPath )
        {
            string filePath = String.Empty;

            do
            {
                var files = Directory.GetFiles( directoryPath, "*.ckmon", SearchOption.TopDirectoryOnly );
                if( files.Length > 0 )
                {
                    // Check if file exists
                    filePath = files[0];
                }
                else
                {
                    Thread.Sleep( 200 );
                }
            } while( String.IsNullOrEmpty( filePath ) );

            WaitForFileRelease( filePath );

            return filePath;
        }

        void WaitForFileRelease( string filePath )
        {
            bool isInUse = false;
            FileStream stream = null;
            do
            {
                try
                {
                    stream = File.Open( filePath, FileMode.Open, FileAccess.Read, FileShare.None );
                    isInUse = false;
                }
                catch( IOException )
                {
                    isInUse = true;
                    Thread.Sleep( 300 );
                }
                finally
                {
                    if( stream != null )
                        stream.Close();
                }
            } while( isInUse == true );
        }
    }
}
