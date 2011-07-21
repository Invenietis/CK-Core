using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;

namespace Core
{

    [TestFixture]
    public class CoreExtensionTest
    {
        [Test]
        public void TestLog2()
        {
            // Stupid raw test to avoid a buggy test ;-)
            Assert.That(Util.Log2(1) == 0);
            Assert.That(Util.Log2(2) == 1);
            Assert.That(Util.Log2(4) == 2);
            Assert.That(Util.Log2(8) == 3);
            Assert.That(Util.Log2(16) == 4);
            Assert.That(Util.Log2(32) == 5);
            Assert.That(Util.Log2(64) == 6);
            Assert.That(Util.Log2(128) == 7);
            Assert.That(Util.Log2(256) == 8);
            Assert.That(Util.Log2(512) == 9);
            Assert.That(Util.Log2(1024) == 10);
            Assert.That(Util.Log2(2048) == 11);
            Assert.That(Util.Log2(4096) == 12);
            Assert.That(Util.Log2(8192) == 13);
            Assert.That(Util.Log2(16384) == 14);
            Assert.That(Util.Log2(32768) == 15);
            Assert.That(Util.Log2(65536) == 16);
            Assert.That(Util.Log2(131072) == 17);
            Assert.That(Util.Log2(262144) == 18);
            Assert.That(Util.Log2(524288) == 19);
            Assert.That(Util.Log2(1048576) == 20);
            Assert.That(Util.Log2(2097152) == 21);
            Assert.That(Util.Log2(4194304) == 22);
            Assert.That(Util.Log2(8388608) == 23);
            Assert.That(Util.Log2(16777216) == 24);
            Assert.That(Util.Log2(33554432) == 25);
            Assert.That(Util.Log2(67108864) == 26);
            Assert.That(Util.Log2(134217728) == 27);
            Assert.That(Util.Log2(268435456) == 28);
            Assert.That(Util.Log2(536870912) == 29);
            Assert.That(Util.Log2(1073741824) == 30);
            Assert.That(Util.Log2(2147483648) == 31);

            Assert.That(Util.Log2(4 + 2 + 1) == 2);
            Assert.That(Util.Log2(64 + 25) == 6);
            Assert.That(Util.Log2(512 + 255) == 9);
            Assert.That(Util.Log2(65536 + 8723) == 16);
            Assert.That(Util.Log2(67108864 + 72868) == 26);
            Assert.That(Util.Log2(536870912 + 897397) == 29);
            Assert.That(Util.Log2(2147483648 + 76575) == 31);

            Assert.That(Util.Log2ForPower2(4) == 2);
            Assert.That(Util.Log2ForPower2(64) == 6);
            Assert.That(Util.Log2ForPower2(512) == 9);
            Assert.That(Util.Log2ForPower2(65536) == 16);
            Assert.That(Util.Log2ForPower2(67108864) == 26);
            Assert.That(Util.Log2ForPower2(536870912) == 29);
            Assert.That(Util.Log2ForPower2(2147483648) == 31);

            Assert.That(Util.Log2ForPower2(4 + 2 + 1) != 2);
            Assert.That(Util.Log2ForPower2(64 + 25) != 6);
            Assert.That(Util.Log2ForPower2(512 + 255) != 9);
            Assert.That(Util.Log2ForPower2(65536 + 8723) != 16);
            Assert.That(Util.Log2ForPower2(67108864 + 72868) != 26);
            Assert.That(Util.Log2ForPower2(536870912 + 897397) != 29);
            Assert.That(Util.Log2ForPower2(2147483648 + 9879633) != 31);

        }
    }
}
