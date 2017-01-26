using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CK.Core.Tests.Monitoring
{
    public class MutexTest<T> : IDisposable
    {
        static readonly object _lock = new object();
        static readonly object _lockFact = new object();

        public MutexTest()
        {
            Monitor.Enter(_lock);
        }

        void IDisposable.Dispose()
        {
            OnDispose();
            Monitor.Exit( _lock );
        }

        class Release : IDisposable
        {
            public void Dispose()
            {
                Monitor.Exit(_lockFact);
            }
        }
        static Release _releaser = new Release();

        protected IDisposable LockFact()
        {
            Monitor.Enter(_lockFact);
            return _releaser;
        }

        protected virtual void OnDispose()
        {
        }
    }
}
