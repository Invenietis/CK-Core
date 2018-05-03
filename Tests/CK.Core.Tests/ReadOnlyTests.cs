using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;

namespace CK.Core.Tests.Collection
{
    public class TestCollection<T> : ICKReadOnlyCollection<T>
    {
        public List<T> Content;

        public event EventHandler CountCalled;
        public event EventHandler ContainsCalled;

        public TestCollection()
        {
            Content = new List<T>();
        }

        public bool Contains(object item)
        {
            if (ContainsCalled != null) ContainsCalled(this, EventArgs.Empty);
            return item is T ? Content.Contains((T)item) : false;
        }

        public int Count
        {
            get
            {
                if (CountCalled != null) CountCalled(this, EventArgs.Empty);
                return Content.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }

    }

    public class TestCollectionThatImplementsICollection<T> : ICKReadOnlyCollection<T>, ICollection<T>
    {
        public List<T> Content;

        public event EventHandler CountCalled;
        public event EventHandler ContainsCalled;

        public TestCollectionThatImplementsICollection()
        {
            Content = new List<T>();
        }

        public bool Contains(object item)
        {
            if (ContainsCalled != null) ContainsCalled(this, EventArgs.Empty);
            return item is T ? Content.Contains((T)item) : false;
        }

        public int Count
        {
            get
            {
                if (CountCalled != null) CountCalled(this, EventArgs.Empty);
                return Content.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Content.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Content.GetEnumerator();
        }


        #region ICollection<T> Members

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T item)
        {
            return Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }


    public class ReadOnlyTests
    {
        [Test]
        public void linq_with_mere_IReadOnlyCollection_implementation_is_not_optimal_for_Count()
        {
            TestCollection<int> c = new TestCollection<int>();
            c.Content.Add(2);

            bool containsCalled = false, countCalled = false;
            c.ContainsCalled += (o, e) => { containsCalled = true; };
            c.CountCalled += (o, e) => { countCalled = true; };

            c.Count.Should().Be(1);
            countCalled.Should().BeTrue("Count property on the concrete type logs the calls."); countCalled = false;

            c.Count().Should().Be(1, "Use Linq extension methods (on the concrete type).");
            countCalled.Should().BeFalse("The Linq extension method did NOT call our Count.");

            IEnumerable<int> cLinq = c;

            cLinq.Count().Should().Be(1, "Linq can not use our implementation...");
            countCalled.Should().BeFalse("...it did not call our Count property.");

            // Addressing the concrete type: it is our method that is called.
            c.Contains(2).Should().BeTrue();
            containsCalled.Should().BeTrue("It is our Contains method that is called (not the Linq one)."); containsCalled = false;
            c.Contains(56).Should().BeFalse();
            containsCalled.Should().BeTrue("It is our Contains method that is called."); containsCalled = false;
            c.Contains(null).Should().BeFalse("Contains should accept ANY object without any error.");
            containsCalled.Should().BeTrue("It is our Contains method that is called."); containsCalled = false;

            // Unfortunately, addressing the IEnumerable base type, Linq has no way to use our methods...
            cLinq.Contains(2).Should().BeTrue();
            containsCalled.Should().BeFalse("Linq use the enumerator to do the job.");
            cLinq.Contains(56).Should().BeFalse();
            containsCalled.Should().BeFalse();
            // Linq Contains() accept only parameter of the generic type.
            // !cLinq.Contains( null ), "Contains should accept ANY object without any error." );
        }

        [Test]
        public void linq_on_ICollection_implementation_uses_Count_property()
        {
            TestCollectionThatImplementsICollection<int> c = new TestCollectionThatImplementsICollection<int>();
            c.Content.Add(2);

            bool containsCalled = false, countCalled = false;
            c.ContainsCalled += (o, e) => { containsCalled = true; };
            c.CountCalled += (o, e) => { countCalled = true; };

            c.Should().HaveCount(1);
            countCalled.Should().BeTrue("Count property on the concrete type logs the calls."); countCalled = false;

            IEnumerable<int> cLinq = c;

            cLinq.Count().Should().Be(1, "Is it our Count implementation that is called?");
            countCalled.Should().BeTrue("Yes!"); countCalled = false;

            c.Count.Should().Be(1, "Linq DOES use our implementation...");
            countCalled.Should().BeTrue("...our Count property has been called."); countCalled = false;

            // What's happening for Contains? 
            // The ICollection<T>.Contains( T ) is more precise than our Contains( object )...

            // Here we target the concrete type.
            c.Contains(2).Should().BeTrue();
            containsCalled.Should().BeTrue("It is our Contains method that is called (not the Linq one)."); containsCalled = false;

            // Here we use the IEnumerable<int>. 
            // It shows that this is not the (slow) enumeration that is used here: it uses a direct call to Contains that can be much more efficient.
            // It works only because TestCollectionThatImplementsICollection relays the call to our Contains.
            cLinq.Contains(2).Should().BeTrue();
            containsCalled.Should().BeTrue("It is our Contains method that is called (not the Linq one)."); containsCalled = false;

            cLinq.Contains(56).Should().BeFalse();
            containsCalled.Should().BeTrue("It is our Contains method that is called."); containsCalled = false;

        }

        [Test]
        public void covariant_Contains_accepts_any_types()
        {
            TestCollection<Animal> c = new TestCollection<Animal>();
            Animal oneElement = new Animal(null);
            c.Content.Add(oneElement);

            bool containsCalled = false;
            c.ContainsCalled += (o, e) => { containsCalled = true; };
            c.Contains(oneElement).Should().BeTrue();
            containsCalled.Should().BeTrue("It is our Contains method that is called."); containsCalled = false;
            c.Contains(56).Should().BeFalse("Contains should accept ANY object without any error.");
            containsCalled.Should().BeTrue("It is our Contains method that is called."); containsCalled = false;
            c.Contains(null).Should().BeFalse("Contains should accept ANY object without any error.");
            containsCalled.Should().BeTrue(); containsCalled = false;
        }

        class StringInt : IComparable<int>
        {
            public readonly string Value;
            public StringInt(string value) { Value = value; }

            public int CompareTo(int other)
            {
                return Int32.Parse(Value).CompareTo(other);
            }
        }

        
        [TestCase("", 5, ~0)]
        [TestCase("1", 5, ~1)]
        [TestCase("1", -5, ~0)]
        [TestCase("1,2,5", 5, 2)]
        [TestCase("1,2,5", 4, ~2)]
        [TestCase("1,2,5", 2, 1)]
        [TestCase("1,2,5", 1, 0)]
        [TestCase("1,2,5", 0, ~0)]
        public void BinarySearch_on_IComparable_TValue_items(string values, int search, int resultIndex)
        {
            var a = values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(v => new StringInt(v)).ToArray();
            Util.BinarySearch(a, search).Should().Be(resultIndex);
        }

        [Test]
        public void IndexOf_on_IReadOnlyList()
        {
            IReadOnlyList<int> l = new[] { 3, 7, 9, 1, 3, 8 };
            l.IndexOf(i => i == 3).Should().Be(0);
            l.IndexOf(i => i == 7).Should().Be(1);
            l.IndexOf(i => i == 8).Should().Be(5);
            l.IndexOf(i => i == 0).Should().Be(-1);
            l.Invoking( sut => sut.IndexOf(null)).Should().Throw<ArgumentNullException>();
        }

    }
}
