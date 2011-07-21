using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace Core
{
    [TestFixture]
    public class EnumerableConverter
    {
        string Converter(int lenght)
        {
            StringBuilder sb = new StringBuilder(lenght);
            for (int i = 0; i < lenght; i++)
            {
                sb.Append('a');
            }
            return sb.ToString();
        }

        [Test]
        public void TestConverter()
        {
            Assert.That(Converter(5).Length == 5);
        }

        [Test]
        public void ConvertIntCollectionToStringCollection()
        {
            int[] intCollection = new int[] { 1, 2, 3, 4, 5, 6 };

            IEnumerator<string> stringEnumerator = Wrapper<string>.CreateEnumerator<int>(intCollection, Converter);
            IEnumerator objectEnumerator = Wrapper<object>.CreateEnumerator<int>(intCollection, Converter);

            int i = 0;
            while (stringEnumerator.MoveNext())
            {
                Assert.That(stringEnumerator.Current.Length, Is.EqualTo(intCollection[i]));
                i++;
            }

            i = 0;
            while (objectEnumerator.MoveNext())
            {
                Assert.That(objectEnumerator.Current is string);
                Assert.That(((string)objectEnumerator.Current).Length, Is.EqualTo(intCollection[i]));
                i++;
            }
        }
    }
}
