using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.Core
{
    public class CKReandOnlyCollectionOnICollection<T> : IReadOnlyCollection<T>
    {
        public CKReandOnlyCollectionOnICollection( ICollection<T> values )
        {
            Values = values;
        }

        public ICollection<T> Values { get; set; }

        public int Count => Values.Count;

        public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
