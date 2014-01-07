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
        [Test]
        public void LogEntryReadWrite()
        {
            var exInner = new CKExceptionData( "message", "typeof(exception)", "assemblyQualifiedName", "stackTrace", null, "fileName", "fusionLog", null, null );
            var ex2 = new CKExceptionData( "message2", "typeof(exception2)", "assemblyQualifiedName2", "stackTrace2", exInner, "fileName2", "fusionLog2", null, null );
            var exL = new CKExceptionData( "loader-message", "typeof(loader-exception)", "loader-assemblyQualifiedName", "loader-stackTrace", null, "loader-fileName", "loader-fusionLog", null, null );
            var exAgg = new CKExceptionData( "agg-message", "typeof(agg-exception)", "agg-assemblyQualifiedName", "agg-stackTrace", ex2, "fileName", "fusionLog", null, new[]{ ex2, exL } );

            ILogEntry e1 = LogEntry.CreateLog( "Text1", DateTime.UtcNow, LogLevel.Info, "c:\\test.cs", 3712, ActivityMonitor.Tags.CreateDependentActivity, exAgg );
            ILogEntry e2 = LogEntry.CreateMulticastLog( Guid.Empty, 5, "Text2", DateTime.UtcNow, LogLevel.Info, "c:\\test.cs", 3712, ActivityMonitor.Tags.CreateDependentActivity, exAgg );

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
                    Assert.That( reader.Current.LogTimeUtc, Is.EqualTo( e1.LogTimeUtc ) );
                    Assert.That( reader.Current.Exception.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( e1.Exception.ExceptionTypeAssemblyQualifiedName ) );
                    Assert.That( reader.MoveNext() );
                    Assert.That( reader.Current.Text, Is.EqualTo( e2.Text ) );
                    Assert.That( reader.Current.LogTimeUtc, Is.EqualTo( e2.LogTimeUtc ) );
                    Assert.That( reader.Current.Exception.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( e2.Exception.ExceptionTypeAssemblyQualifiedName ) );
                    Assert.That( reader.MoveNext(), Is.False );
                }
            }

        }

    }
}
