using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Diagnostics;

namespace CK.Core
{

    [TestFixture]
    public class MissingDisposeCallSentinelTests
    {

        class DisposableClassDebug : IDisposable
        {

#if DEBUG
            MissingDisposeCallSentinel _sentinel = new MissingDisposeCallSentinel();
            ~DisposableClassDebug()
            {
                MissingDisposeCallSentinel.RegisterMissing( _sentinel );
            }
#endif

            public void Dispose()
            {
#if DEBUG
                _sentinel = null;
                GC.SuppressFinalize( this );
#endif
            }
        }

        [Test]
        public void TestWorksInDebug1()
        {
            MissingDisposeCallSentinel.Clear();
            var c = new DisposableClassDebug();
            c = null;
            GC.Collect( 1, GCCollectionMode.Forced );
            GC.WaitForPendingFinalizers();
            #if DEBUG
            Assert.That( MissingDisposeCallSentinel.Missing.Count() == 1 );
            MissingDisposeCallSentinel.Clear();
            #else
            Assert.That( MissingDisposeCallSentinel.Missing.Count() == 0 );
            #endif
        }

        [Test]
        public void TestWorksInDebug2()
        {
            MissingDisposeCallSentinel.Clear();
            var c = new DisposableClassDebug();
            c = null;
            string missing = null;
            MissingDisposeCallSentinel.DebugCheckMissing( s => missing = s );
            
            #if DEBUG
            Assert.That( missing != null, "Missing Dispose detected." );
            MissingDisposeCallSentinel.Clear();
            #else
            Assert.That( missing == null, "Missing Dispose NOT detected." );
            #endif      
        }

        [Test]
        public void TestSuccess()
        {
            MissingDisposeCallSentinel.Clear();
            using( var c = new DisposableClassDebug() )
            {
            }
            GC.Collect( 1, GCCollectionMode.Forced );
            GC.WaitForPendingFinalizers();
            Assert.That( MissingDisposeCallSentinel.Missing.Count() == 0 );
        }
    }
}
