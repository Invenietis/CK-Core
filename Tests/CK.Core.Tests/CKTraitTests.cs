using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace CK.Core.Tests
{

    /// <summary>
    /// This class test operations on CKTrait (FindOrCreate, Intersect, etc.).
    /// </summary>
    public class CKTraitTests
    {
        static CKTraitContext ContextWithPlusSeparator() => CKTraitContext.Create( "Test", '+' );

        [Test]
        public void Comparing_tags()
        {
            CKTraitContext c1 = CKTraitContext.Create( "C1" );
            CKTraitContext c2 = CKTraitContext.Create( "C2" );

            c1.CompareTo( c1 ).Should().Be( 0 );
            c1.CompareTo( c2 ).Should().BeLessThan( 0 );

            var tAc1 = c1.FindOrCreate( "A" );
            var tBc1 = c1.FindOrCreate( "B" );
            var tABc1 = c1.FindOrCreate( "A|B" );
            var tAc2 = c2.FindOrCreate( "A" );

            tAc1.CompareTo( tAc1 ).Should().Be( 0 );
            tAc1.CompareTo( tBc1 ).Should().BeGreaterThan( 0, "In the same context, A is stronger than B." );
            tABc1.CompareTo( tBc1 ).Should().BeGreaterThan( 0, "In the same context, A|B is stronger than B." );
            tAc1.CompareTo( tAc2 ).Should().BeLessThan( 0, "Between different contexts, the context ordering drives the ordering." );
            tABc1.CompareTo( tAc2 ).Should().BeLessThan( 0, "Between different contexts, the context ordering drives the ordering." );
        }

        [Test]
        public void Tags_must_belong_to_the_same_context()
        {
            Action a = () => CKTraitContext.Create( null! );
            a.Should().Throw<ArgumentException>();

            a = () => CKTraitContext.Create( "  " );
            a.Should().Throw<ArgumentException>();

            CKTraitContext c1 = CKTraitContext.Create( "C1" );
            CKTraitContext c2 = CKTraitContext.Create( "C2" );

            var t1 = c1.FindOrCreate( "T1" );
            var t2 = c2.FindOrCreate( "T2" );
            t1.Should().NotBeSameAs( t2 );
            t1.Invoking( sut => sut.Union( t2 ) ).Should().Throw<ArgumentException>();
            t1.Invoking( sut => sut.Intersect( t2 ) ).Should().Throw<ArgumentException>();
            t1.Invoking( sut => sut.Except( t2 ) ).Should().Throw<ArgumentException>();
            t1.Invoking( sut => sut.SymmetricExcept( t2 ) ).Should().Throw<ArgumentException>();

            t1.Invoking( sut => sut.Overlaps( t2 ) ).Should().Throw<ArgumentException>();
            t1.Invoking( sut => sut.IsSupersetOf( t2 ) ).Should().Throw<ArgumentException>();

            t1.Invoking( sut => sut.Union( null! ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.Intersect( null! ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.Except( null! ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.SymmetricExcept( null! ) ).Should().Throw<ArgumentNullException>();

            t1.Invoking( sut => sut.Overlaps( null! ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.IsSupersetOf( null! ) ).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void EmptyTag_is_everywhere()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m = c.EmptyTrait;
            m.ToString().Should().BeSameAs( string.Empty, "Empty tag is the empty string." );
            m.IsAtomic.Should().BeTrue( "Empty tag is considered as atomic." );
            m.AtomicTraits.Should().BeEmpty( "Empty tag has no atomic tags inside." );

            c.FindOrCreate( null! ).Should().BeSameAs( m, "Null gives the empty tag." );
            c.FindOrCreate( "" ).Should().BeSameAs( m, "Obtaining empty string gives the empty tag." );
            c.FindOrCreate( "+" ).Should().BeSameAs( m, "Obtaining '+' gives the empty tag." );
            c.Invoking( sut => sut.FindOrCreate( " \t \n  " ) ).Should().Throw<ArgumentException>( "No \n inside." );
            c.Invoking( sut => sut.FindOrCreate( " \r " ) ).Should().Throw<ArgumentException>( "No \r inside." );
            c.FindOrCreate( "+ \t +" ).Should().BeSameAs( m, "Leading and trailing '+' are ignored." );
            c.FindOrCreate( "++++" ).Should().BeSameAs( m, "Multiple + are ignored" );
            c.FindOrCreate( "++  +++  + \t +" ).Should().BeSameAs( m, "Multiple empty strings leads to empty tag." );

            c.FindOnlyExisting( null! ).Should().BeNull();
            c.FindOnlyExisting( "" ).Should().BeNull();
            c.FindOnlyExisting( " " ).Should().BeNull();
            c.FindOnlyExisting( " ++  + " ).Should().BeNull();
            c.FindOnlyExisting( "NONE" ).Should().BeNull();
            c.FindOnlyExisting( "NO+NE" ).Should().BeNull();
            c.FindOnlyExisting( "N+O+N+E" ).Should().BeNull();
        }

        [Test]
        public void test_AtomicTag_parsing()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m = c.FindOrCreate( "Alpha" );
            m.IsAtomic.Should().BeTrue();
            m.AtomicTraits.Count.Should().Be( 1, "Not a combined one." );
            m.AtomicTraits[0].Should().BeSameAs( m, "Atomic tags are self-contained." );

            c.FindOrCreate( " \t Alpha\t\t  " ).Should().BeSameAs( m, "Strings are trimmed." );
            c.FindOrCreate( "+ \t Alpha+" ).Should().BeSameAs( m, "Leading and trailing '+' are ignored." );
            c.FindOrCreate( "+Alpha+++" ).Should().BeSameAs( m, "Multiple + are ignored" );
            c.FindOrCreate( "++ Alpha +++ \t\t  + \t +" ).Should().BeSameAs( m, "Multiple empty strings are ignored." );

            var notExist1 = Guid.NewGuid().ToString();
            var notExist2 = Guid.NewGuid().ToString();
            c.FindOnlyExisting( notExist1 ).Should().BeNull();
            c.FindOnlyExisting( $"{notExist1}+{notExist2}" ).Should().BeNull();
            c.FindOnlyExisting( "Alpha" ).Should().BeSameAs( m );
            c.FindOnlyExisting( $"{notExist1}+{notExist2}+Alpha" ).Should().BeSameAs( m );
        }

        [Test]
        public void test_Combined_tags_parsing()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m = c.FindOrCreate( "Beta+Alpha" );
            m.IsAtomic.Should().BeFalse();
            m.AtomicTraits.Should().HaveCount( 2, "Combined tag." );
            m.AtomicTraits[0].Should().BeSameAs( c.FindOrCreate( "Alpha" ), "Atomic Alpha is the first one." );
            m.AtomicTraits[1].Should().BeSameAs( c.FindOrCreate( "Beta" ), "Atomic Beta is the second one." );

            c.FindOrCreate( "Alpha+Beta" ).Should().BeSameAs( m, "Canonical order is ensured." );
            c.FindOrCreate( "+ +\t++ Alpha+++Beta++" ).Should().BeSameAs( m, "Extra characters and empty tags are ignored." );

            c.FindOrCreate( "Alpha+Beta+Alpha" ).Should().BeSameAs( m, "Multiple identical tags are removed." );
            c.FindOrCreate( "Alpha+ +Beta\t ++Beta+ + Alpha +    Beta   ++ " )
                .Should().BeSameAs( m, "Multiple identical tags are removed." );

            CKTrait m2 = c.FindOrCreate( "Beta+Alpha+Zeta+Tau+Pi+Omega+Epsilon" );
            c.FindOrCreate( "++Beta+Zeta+Omega+Epsilon+Alpha+Zeta+Epsilon+Zeta+Tau+Epsilon+Pi+Tau+Beta+Zeta+Omega+Beta+Pi+Alpha" )
                .Should().BeSameAs( m2, "Unicity of Atomic tag is ensured." );

            var notExists1 = Guid.NewGuid().ToString();
            var notExists2 = Guid.NewGuid().ToString();
            var notExists3 = Guid.NewGuid().ToString();
            c.FindOnlyExisting( "Beta" )!.ToString().Should().Be( "Beta" );
            c.FindOnlyExisting( $"Beta+{notExists1}" )!.ToString().Should().Be( "Beta" );
            c.FindOnlyExisting( $"Beta+{notExists1}+{notExists2}+Alpha+{notExists3}" )!.Should().BeSameAs( m );
            c.FindOnlyExisting( $"Beta+  {notExists1} + {notExists2} +Alpha+{notExists3}+Tau+Pi" )!.ToString().Should().Be( "Alpha+Beta+Pi+Tau" );
        }

        [Test]
        public void test_FindOnlyExisting_with_its_optional_collector()
        {
            var c = ContextWithPlusSeparator();

            List<string> collector = new List<string>();
            c.FindOrCreate( "Beta+Alpha+Tau+Pi" );

            var noExists1 = "A" + Guid.NewGuid().ToString();
            var noExists2 = "B" + Guid.NewGuid().ToString();
            var noExists3 = "C" + Guid.NewGuid().ToString();
            var noExists4 = "D" + Guid.NewGuid().ToString();
            var f = c.FindOnlyExisting( $"Beta+{noExists1}+{noExists2}+Alpha+{noExists3}+Tau+Pi+{noExists4}", t => { collector.Add( t ); return true; } );
            Debug.Assert( f != null );
            f.ToString().Should().Be( "Alpha+Beta+Pi+Tau" );
            String.Join( ",", collector ).Should().Be( $"{noExists1},{noExists2},{noExists3},{noExists4}" );

            collector.Clear();
            f = c.FindOnlyExisting( $"Beta+{noExists1}+{noExists2}+Alpha+{noExists3}+Tau+Pi", t => { collector.Add( t ); return t != noExists3; } );
            Debug.Assert( f != null );
            f.ToString().Should().Be( "Alpha+Beta" );
            String.Join( ",", collector ).Should().Be( $"{noExists1},{noExists2},{noExists3}" );
        }

        [Test]
        public void test_Intersect_between_tags()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m1 = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = c.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            m1.Intersect( m2 ).ToString().Should().Be( "Combo+Fridge", "Works as expected :-)" );
            m2.Intersect( m1 ).Should().BeSameAs( m1.Intersect( m2 ), "Same object in both calls." );

            m2.Intersect( c.EmptyTrait ).Should().BeSameAs( c.EmptyTrait, "Intersecting empty gives empty." );
        }

        [Test]
        public void test_Union_of_tags()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m1 = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = c.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            m1.Union( m2 ).ToString().Should().Be( "Alpha+Alt+Another+Beta+Combo+Fridge+Xtra", "Works as expected :-)" );
            m2.Union( m1 ).Should().BeSameAs( m1.Union( m2 ), "Same in both calls." );
        }

        [Test]
        public void test_Except_of_tags()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m1 = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = c.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            m1.Except( m2 ).ToString().Should().Be( "Alpha+Beta", "Works as expected :-)" );
            m2.Except( m1 ).ToString().Should().Be( "Alt+Another+Xtra", "Works as expected..." );

            m2.Except( c.EmptyTrait ).Should().BeSameAs( m2, "Removing empty does nothing." );
            m1.Except( c.EmptyTrait ).Should().BeSameAs( m1, "Removing empty does nothing." );
        }


        [Test]
        public void Contains_is_IsSupersetOf()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );

            c.EmptyTrait.IsSupersetOf( c.EmptyTrait ).Should().BeTrue( "Empty is contained by definition in itself." );
            m.IsSupersetOf( c.EmptyTrait ).Should().BeTrue( "Empty is contained by definition." );
            m.IsSupersetOf( c.FindOrCreate( "Fridge+Alpha" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge+Alpha+Combo" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge+Alpha+Beta+Combo" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge+Lol" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Murfn" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Fridge+Alpha+Combo+Lol" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Lol+Fridge+Alpha+Beta+Combo" ) ).Should().BeFalse();

            m.Overlaps( c.FindOrCreate( "Fridge+Alpha" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "Nimp+Fridge+Mourfn" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "Fridge+Alpha+Combo+Albert" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "ZZF+AAlp+BBeBe+Combo" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "AFridge+ALol" ) ).Should().BeFalse();
            m.Overlaps( c.FindOrCreate( "Murfn" ) ).Should().BeFalse();
            m.Overlaps( c.FindOrCreate( "QF+QA+QC+QL" ) ).Should().BeFalse();
            m.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' since EmptyTag.AtomicTags.Count == 0..." );
            c.EmptyTrait.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' in itself." );
        }

        [Test]
        public void tag_separator_can_be_changed_from_the_default_pipe()
        {
            var c = CKTraitContext.Create( "SemiColonContext", ';' );
            CKTrait m = c.FindOrCreate( "Beta;Alpha;Fridge;Combo" );

            c.EmptyTrait.IsSupersetOf( c.EmptyTrait ).Should().BeTrue( "Empty is contained by definition in itself." );
            m.IsSupersetOf( c.EmptyTrait ).Should().BeTrue( "Empty is contained by definition." );
            m.IsSupersetOf( c.FindOrCreate( "Fridge;Alpha" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge;Alpha;Combo" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge;Alpha;Beta;Combo" ) ).Should().BeTrue();
            m.IsSupersetOf( c.FindOrCreate( "Fridge;Lol" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Murfn" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Fridge;Alpha;Combo+Lol" ) ).Should().BeFalse();
            m.IsSupersetOf( c.FindOrCreate( "Lol;Fridge;Alpha;Beta;Combo" ) ).Should().BeFalse();

            m.Overlaps( c.FindOrCreate( "Fridge;Alpha" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "Nimp;Fridge;Mourfn" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "Fridge;Alpha;Combo;Albert" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "ZZF;AAlp;BBeBe;Combo" ) ).Should().BeTrue();
            m.Overlaps( c.FindOrCreate( "AFridge;ALol" ) ).Should().BeFalse();
            m.Overlaps( c.FindOrCreate( "Murfn" ) ).Should().BeFalse();
            m.Overlaps( c.FindOrCreate( "QF;QA;QC;QL" ) ).Should().BeFalse();
            m.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' since EmptyTag.AtomicTags.Count == 0..." );
            c.EmptyTrait.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' in itself." );
        }

        [Test]
        public void Toggle_is_SymmetricExcept()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            m.SymmetricExcept( c.FindOrCreate( "Beta" ) ).ToString().Should().Be( "Alpha+Combo+Fridge" );
            m.SymmetricExcept( c.FindOrCreate( "Fridge+Combo" ) ).ToString().Should().Be( "Alpha+Beta" );
            m.SymmetricExcept( c.FindOrCreate( "Beta+Fridge+Combo" ) ).ToString().Should().Be( "Alpha" );
            m.SymmetricExcept( c.FindOrCreate( "Beta+Fridge+Combo+Alpha" ) ).ToString().Should().Be( "" );

            m.SymmetricExcept( c.FindOrCreate( "" ) ).ToString().Should().Be( "Alpha+Beta+Combo+Fridge" );
            m.SymmetricExcept( c.FindOrCreate( "Xtra" ) ).ToString().Should().Be( "Alpha+Beta+Combo+Fridge+Xtra" );
            m.SymmetricExcept( c.FindOrCreate( "Alpha+Xtra" ) ).ToString().Should().Be( "Beta+Combo+Fridge+Xtra" );
            m.SymmetricExcept( c.FindOrCreate( "Zenon+Alpha+Xtra+Fridge" ) ).ToString().Should().Be( "Beta+Combo+Xtra+Zenon" );
        }

        [Test]
        public void Fallbacks_generation()
        {
            var c = ContextWithPlusSeparator();
            {
                CKTrait m = c.FindOrCreate( "" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                m.FallbacksCount.Should().Be( f.Count );
                f.Count.Should().Be( 1 );
                f[0].ToString().Should().Be( "" );
            }
            {
                CKTrait m = c.FindOrCreate( "Alpha" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                m.FallbacksCount.Should().Be( f.Count );
                f.Count.Should().Be( 1 );
                f[0].ToString().Should().Be( "" );
            }
            {
                CKTrait m = c.FindOrCreate( "Alpha+Beta" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                m.FallbacksCount.Should().Be( f.Count );
                f.Count.Should().Be( 3 );
                f[0].ToString().Should().Be( "Alpha" );
                f[1].ToString().Should().Be( "Beta" );
                f[2].ToString().Should().Be( "" );
            }
            {
                CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                m.FallbacksCount.Should().Be( f.Count );
                f.Count.Should().Be( 7 );
                f[0].ToString().Should().Be( "Alpha+Beta" );
                f[1].ToString().Should().Be( "Alpha+Combo" );
                f[2].ToString().Should().Be( "Beta+Combo" );
                f[3].ToString().Should().Be( "Alpha" );
                f[4].ToString().Should().Be( "Beta" );
                f[5].ToString().Should().Be( "Combo" );
                f[6].ToString().Should().Be( "" );
            }
            {
                CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                m.FallbacksCount.Should().Be( f.Count );
                f.Count.Should().Be( 15 );
                f[0].ToString().Should().Be( "Alpha+Beta+Combo" );
                f[1].ToString().Should().Be( "Alpha+Beta+Fridge" );
                f[2].ToString().Should().Be( "Alpha+Combo+Fridge" );
                f[3].ToString().Should().Be( "Beta+Combo+Fridge" );
                f[4].ToString().Should().Be( "Alpha+Beta" );
                f[5].ToString().Should().Be( "Alpha+Combo" );
                f[6].ToString().Should().Be( "Alpha+Fridge" );
                f[7].ToString().Should().Be( "Beta+Combo" );
                f[8].ToString().Should().Be( "Beta+Fridge" );
                f[9].ToString().Should().Be( "Combo+Fridge" );
                f[10].ToString().Should().Be( "Alpha" );
                f[11].ToString().Should().Be( "Beta" );
                f[12].ToString().Should().Be( "Combo" );
                f[13].ToString().Should().Be( "Fridge" );
                f[14].ToString().Should().Be( "" );
            }
        }

        [Test]
        public void Fallbacks_ordering()
        {
            var c = ContextWithPlusSeparator();
            {
                CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();

                CKTrait[] sorted = f.ToArray();
                Array.Sort( sorted );
                Array.Reverse( sorted );
                sorted.SequenceEqual( f ).Should().BeTrue( "CKTrait.CompareTo respects the fallbacks (fallbacks is in reverse order)." );
            }
            {
                CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo+Fridge+F+K+Ju+J+A+B" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                f.OrderBy( tag => tag ).Reverse().SequenceEqual( f ).Should().BeTrue( "CKTrait.CompareTo is ok." );
            }
            {
                CKTrait m = c.FindOrCreate( "xz+lz+ded+az+zer+t+zer+ce+ret+ert+ml+a+nzn" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                f.OrderBy( tag => tag ).Reverse().SequenceEqual( f ).Should().BeTrue( "CKTrait.CompareTo is ok." );
            }
        }

        [Test]
        public void FindIfAllExist_tests()
        {
            var c = CKTraitContext.Create( "Indep", '+', shared: false );

            CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo+Fridge" );

            c.FindIfAllExist( "" ).Should().Be( c.EmptyTrait );
            c.FindIfAllExist( "bo" ).Should().BeNull();
            var alpha = c.FindOrCreate( "Alpha" );
            c.FindIfAllExist( "Alpha" ).Should().Be( alpha );
            c.FindIfAllExist( "bo+pha" ).Should().BeNull();
            c.FindIfAllExist( "Fridge+Combo+Alpha+Beta" ).Should().BeSameAs( m );
        }

        [Test]
        public void independent_or_shared_contexts()
        {
            var shared = CKTraitContext.Create( "Shared", '+' );
            Action notPossible = () => CKTraitContext.Create( "Shared", '-' );
            notPossible.Should().Throw<InvalidOperationException>();

            var here = shared.FindOrCreate( "Here!" );

            var independent1 = CKTraitContext.Create( "Shared", '-', shared: false );
            independent1.FindOnlyExisting( "Here!" ).Should().BeNull();

            independent1.FindOrCreate( "In Independent n°1" );

            var independent2 = CKTraitContext.Create( "Shared", '-', shared: false );
            independent2.FindOnlyExisting( "Here!" ).Should().BeNull();
            independent2.FindOnlyExisting( "In Independent n°1" ).Should().BeNull();

            var anotherShared = CKTraitContext.Create( "Shared", '+' );
            anotherShared.FindOnlyExisting( "Here!" ).Should().BeSameAs( here );

            (anotherShared == shared).Should().BeTrue();
            (anotherShared != shared).Should().BeFalse();
            anotherShared.Equals( shared ).Should().BeTrue();
            anotherShared.CompareTo( shared ).Should().Be( 0 );

            var here1 = independent1.FindOrCreate( "Here!" );
            var here2 = independent2.FindOrCreate( "Here!" );

            here.Should().NotBeSameAs( here1 );
            here.Should().NotBeSameAs( here2 );
            here1.Should().NotBeSameAs( here2 );

            // Comparisons across contexts: a unique index "sorts" the independent contexts.
            // This index is positive for independent contexts: shared context's tags come first.

            here.CompareTo( here1 ).Should().BeNegative();
            here1.CompareTo( here2 ).Should().BeNegative();
            here.CompareTo( here2 ).Should().BeNegative();

            here1.CompareTo( here ).Should().BePositive();
            here2.CompareTo( here1 ).Should().BePositive();
            here2.CompareTo( here ).Should().BePositive();
        }

        [Test]
        public void comparison_operators_work()
        {
            var c = CKTraitContext.Create( "Indep", '+', shared: false );
            var t1 = c.FindOrCreate( "A" );
            var t2 = c.FindOrCreate( "B" );
            //
            (t1 > t2).Should().BeTrue( "A is better than B." );
            (t1 >= t2).Should().BeTrue();
            (t1 != t2).Should().BeTrue();
            (t1 == t2).Should().BeFalse();
            (t1 < t2).Should().BeFalse();
            (t1 <= t2).Should().BeFalse();
            //
            t2 = null;
            (t1 > t2).Should().BeTrue( "Any tag is better than null." );
            (t1 >= t2).Should().BeTrue();
            (t1 != t2).Should().BeTrue();
            (t1 == t2).Should().BeFalse();
            (t1 < t2).Should().BeFalse();
            (t1 <= t2).Should().BeFalse();
            //
            t1 = null;
            (t1 > t2).Should().BeFalse( "null is the same as null." );
            (t1 < t2).Should().BeFalse();
            (t1 >= t2).Should().BeTrue( "null is the same as null." );
            (t1 <= t2).Should().BeTrue();
            (t1 == t2).Should().BeTrue();
            (t1 != t2).Should().BeFalse();
            //
            t2 = c.FindOrCreate( "B" );
            (t1 > t2).Should().BeFalse( "null is smaller that any tag." );
            (t1 >= t2).Should().BeFalse();
            (t1 != t2).Should().BeTrue();
            (t1 == t2).Should().BeFalse();
            (t1 < t2).Should().BeTrue();
            (t1 <= t2).Should().BeTrue();
        }

        [Test]
        public void comparison_operators_work_cross_contexts()
        {
            var c = CKTraitContext.Create( "Indep", '+', shared: false );
            var cAfter = CKTraitContext.Create( "Indep+", '+', shared: false );
            var t1 = cAfter.FindOrCreate( "B" );
            var t2 = c.FindOrCreate( "B" );
            //
            (t1 > t2).Should().BeTrue( "Tag with same name relies on context's separator and then name ('greater' separator and then name are better)." );
            (t1 >= t2).Should().BeTrue();
            (t1 != t2).Should().BeTrue();
            (t1 == t2).Should().BeFalse();
            (t1 < t2).Should().BeFalse();
            (t1 <= t2).Should().BeFalse();
        }

        [Test]
        public void binary_operators_work()
        {
            var ctx = CKTraitContext.Create( "Indep", ',', shared: false );
            var a = ctx.FindOrCreate( "A" );
            var b = ctx.FindOrCreate( "B" );
            var c = ctx.FindOrCreate( "C" );
            var d = ctx.FindOrCreate( "D" );
            var ab = ctx.FindOrCreate( "A,B" );
            var cd = ctx.FindOrCreate( "C,D" );
            var abc = ctx.FindOrCreate( "A,B,C" );
            var abcd = ctx.FindOrCreate( "A,B,C,D" );
            (ab + cd).Should()
                    .BeSameAs( abcd )
                    .And.BeSameAs( c + d + b + a )
                    .And.BeSameAs( cd + ab )
                    .And.BeSameAs( abc | d )
                    .And.BeSameAs( a | b | c | d );

            (ab & cd).Should().BeSameAs( ctx.EmptyTrait );
            (abc & cd).Should().BeSameAs( c );
            (abcd & cd).Should().BeSameAs( cd );

            (ab ^ cd).Should().BeSameAs( abcd );
            (abc ^ cd).Should().BeSameAs( a | b | d );
            ((a | b | d) ^ cd).Should().BeSameAs( abc );
            (abcd ^ cd).Should().BeSameAs( ab );

            CKTrait v = a;
            v += b;
            v.Should().BeSameAs( ab );
            v += d;
            v.Should().BeSameAs( a | b | d );
            v -= c;
            v.Should().BeSameAs( a | b | d );
            v -= abcd;
            v.IsEmpty.Should().BeTrue();

            v |= abcd;
            v &= cd;
            v.Should().BeSameAs( cd );
            v &= d;
            v.Should().BeSameAs( d );
            v |= abcd;
            v.Should().BeSameAs( abcd );
            v ^= b;
            v.Should().BeSameAs( a | cd );
            v ^= ab;
            v.Should().BeSameAs( b | cd );

            v &= ctx.EmptyTrait;
            v.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void Binary_serialization_of_CKTraitCOntext()
        {
            var ctx = CKTraitContext.Create( "Independent", ',', shared: false );
            ctx.FindOrCreate( "A" );
            ctx.FindOrCreate( "B" );
            ctx.FindOrCreate( "C" );
            ctx.FindOrCreate( "D" );
            ctx.FindOrCreate( "A,B" );

            using( var m = new MemoryStream() )
            using( var w = new CKBinaryWriter( m ) )
            {
                ctx.Write( w, writeAllTags: true );
                m.Position = 0;
                using( var r = new CKBinaryReader( m ) )
                {
                    var ctx2 = CKTraitContext.Read( r );
                    ctx2.FindIfAllExist( "A,B,C,D" ).Should().NotBeNull();
                }
            }
        }
    }
}
