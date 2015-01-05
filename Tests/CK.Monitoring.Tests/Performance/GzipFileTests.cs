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
            int[] fileSizes = new int[] { 5*1024*1024, 10*1024*1024, 30*1024*1024 };
            int[] bufferSizes = new int[] { 1 * 1024, 2 * 1024, 3 * 1024, 4 * 1024, 8 * 1024, 32 * 1024 };

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
                        using( TestHelper.ConsoleMonitor.OpenInfo().Send( "File Size = {0} MiB", fileSize/1024/1024 ) )
                        {
                            foreach( int bufferSize in bufferSizes )
                            {
                                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Buffer Size = {0} bytes", bufferSize ) )
                                {
                                    sw.Start();
                                    FileUtil.CompressFileToGzipFile( tf.Path, tfOut.Path, false, bufferSize );
                                    sw.Stop();
                                    TestHelper.ConsoleMonitor.Info().Send( "-Synchronous Gzip write: {0:######} ms.", sw.ElapsedMilliseconds );
                                    sw.Reset();
                                    sw.Start();
                                    await FileUtil.CompressFileToGzipFileAsync( tf.Path, tfOut.Path, CancellationToken.None, false, bufferSize );
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