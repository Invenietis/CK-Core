using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.Tests
{
    [TestFixture]
    public class DateTimeStampUnicityTests
    {

        [Test]
        public void generating_time_collisions()
        {
            DateTimeStamp[] all = new DateTimeStamp[8192];
            DateTimeStamp current = DateTimeStamp.UtcNow;
            for( int i = 0; i < all.Length; ++i )
            {
                DateTimeStamp next = new DateTimeStamp( current, DateTime.UtcNow );
                all[i] = current = next;
            }
            TimeSpan delta = all[all.Length - 1].TimeUtc - all[0].TimeUtc;
            int collisionCount = all.Count( d => d.Uniquifier != 0 );
            Console.WriteLine( $"Delta = {delta}, Collisions = {collisionCount}." );
        }


        [Test]
        public void generating_forced_time_collisions()
        {
            DateTimeStamp fake = DateTimeStamp.UtcNow;

            Stopwatch w = new Stopwatch();
            w.Start();

            DateTimeStamp[] all = new DateTimeStamp[8192];
            DateTimeStamp current = fake;
            for( int i = 0; i < all.Length; ++i )
            {
                DateTimeStamp next = new DateTimeStamp( current, fake );
                all[i] = current = next;
            }
            TimeSpan delta = all[all.Length - 1].TimeUtc - all[0].TimeUtc;
            int collisionCount = all.Count( d => d.Uniquifier != 0 );
            Console.WriteLine( $"Delta = {delta}, Collisions = {collisionCount}." );

            w.Stop();
            Console.WriteLine( $"Ticks = {w.ElapsedTicks}." );
        }


        [Explicit]
        [TestCase(1_000_000)]
        public void date_time_perf( int max )
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            DateTime c;
            for( int i = 0; i < max; ++i )
            {
                c = DateTime.UtcNow;
            }
            w.Stop();
            Console.WriteLine( $"Ticks = {w.ElapsedTicks}." );
        }
    }
}
