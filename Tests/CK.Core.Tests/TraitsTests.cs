#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Traits.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading;

namespace CK.Core.Tests
{

    /// <summary>
    /// This class test operations on CKTrait (FindOrCreate, Intersect, etc.).
    /// </summary>
    public class TraitsTests
    {
        //CKTraitContext Context;

        //public TraitsTests()
        //{
        //    Context = new CKTraitContext( "Test", '+' );
        //}

        //static object _lock = new object();
        //[SetUp]
        //public void Lock() => Monitor.Enter( _lock );

        //[TearDown]
        //public void Unlock() => Monitor.Exit( _lock );

        CKTraitContext ContextWithPlusSeparator() => new CKTraitContext( "Test", '+' );


        [Test]
        public void Comparing_traits()
        {
            CKTraitContext c1 = new CKTraitContext( "C1" );
            CKTraitContext c1Bis = new CKTraitContext( "C1" );
            CKTraitContext c2 = new CKTraitContext( "C2" );

            c1.CompareTo( c1 ).Should().Be( 0 );
            c1.CompareTo( c2 ).Should().BeLessThan( 0 );
            c1Bis.CompareTo( c1 ).Should().BeGreaterThan( 0 );

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
        public void Traits_must_belong_to_the_same_context()
        {
            Action a = () => new CKTraitContext( null );
            a.Should().Throw<ArgumentException>();

            a = () => new CKTraitContext( "  " );
            a.Should().Throw<ArgumentException>();

            CKTraitContext c1 = new CKTraitContext( "C1" );
            CKTraitContext c2 = new CKTraitContext( "C2" );

            var t1 = c1.FindOrCreate( "T1" );
            var t2 = c2.FindOrCreate( "T2" );
            t1.Should().NotBeSameAs( t2 );
            t1.Invoking( sut => sut.Union( t2 ) ).Should().Throw<InvalidOperationException>();
            t1.Invoking( sut => sut.Intersect( t2 ) ).Should().Throw<InvalidOperationException>();
            t1.Invoking( sut => sut.Except( t2 ) ).Should().Throw<InvalidOperationException>();
            t1.Invoking( sut => sut.SymmetricExcept( t2 ) ).Should().Throw<InvalidOperationException>();

            t1.Invoking( sut => sut.Overlaps( t2 ) ).Should().Throw<InvalidOperationException>();
            t1.Invoking( sut => sut.IsSupersetOf( t2 ) ).Should().Throw<InvalidOperationException>();

            t1.Invoking( sut => sut.Union( null ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.Intersect( null ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.Except( null ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.SymmetricExcept( null ) ).Should().Throw<ArgumentNullException>();

            t1.Invoking( sut => sut.Overlaps( null ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.IsSupersetOf( null ) ).Should().Throw<ArgumentNullException>();

            t1.Invoking( sut => sut.CompareTo( null ) ).Should().Throw<ArgumentNullException>();
            t1.Invoking( sut => sut.CompareTo( null ) ).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void EmptyTrait_is_everywhere()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m = c.EmptyTrait;
            m.ToString().Should().BeSameAs( string.Empty, "Empty trait is the empty string." );
            m.IsAtomic.Should().BeTrue( "Empty trait is considered as atomic." );
            m.AtomicTraits.Should().BeEmpty( "Empty trait has no atomic traits inside." );

            c.FindOrCreate( null ).Should().BeSameAs( m, "Null gives the empty trait." );
            c.FindOrCreate( "" ).Should().BeSameAs( m, "Obtaining empty string gives the empty trait." );
            c.FindOrCreate( "+" ).Should().BeSameAs( m, "Obtaining '+' gives the empty trait." );
            c.Invoking( sut => sut.FindOrCreate( " \t \n  " ) ).Should().Throw<ArgumentException>( "No \n inside." );
            c.Invoking( sut => sut.FindOrCreate( " \r " ) ).Should().Throw<ArgumentException>( "No \r inside." );
            c.FindOrCreate( "+ \t +" ).Should().BeSameAs( m, "Leading and trailing '+' are ignored." );
            c.FindOrCreate( "++++" ).Should().BeSameAs( m, "Multiple + are ignored" );
            c.FindOrCreate( "++  +++  + \t +" ).Should().BeSameAs( m, "Multiple empty strings leads to empty trait." );

            c.FindOnlyExisting( null ).Should().BeNull();
            c.FindOnlyExisting( "" ).Should().BeNull();
            c.FindOnlyExisting( " " ).Should().BeNull();
            c.FindOnlyExisting( " ++  + " ).Should().BeNull();
            c.FindOnlyExisting( "NONE" ).Should().BeNull();
            c.FindOnlyExisting( "NO+NE" ).Should().BeNull();
            c.FindOnlyExisting( "N+O+N+E" ).Should().BeNull();
        }

        [Test]
        public void test_AtomicTrait_parsing()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m = c.FindOrCreate( "Alpha" );
            m.IsAtomic.Should().BeTrue();
            m.AtomicTraits.Count.Should().Be( 1, "Not a combined one." );
            m.AtomicTraits[0].Should().BeSameAs( m, "Atomic traits are self-contained." );

            c.FindOrCreate( " \t Alpha\t\t  " ).Should().BeSameAs( m, "Strings are trimmed." );
            c.FindOrCreate( "+ \t Alpha+" ).Should().BeSameAs( m, "Leading and trailing '+' are ignored." );
            c.FindOrCreate( "+Alpha+++" ).Should().BeSameAs( m, "Multiple + are ignored" );
            c.FindOrCreate( "++ Alpha +++ \t\t  + \t +" ).Should().BeSameAs( m, "Multiple empty strings are ignored." );

            c.FindOnlyExisting( "Beta" ).Should().BeNull();
            c.FindOnlyExisting( "Beta+Gamma" ).Should().BeNull();
            c.FindOnlyExisting( "Alpha" ).Should().BeSameAs( m );
            c.FindOnlyExisting( "Beta+Gamma+Alpha" ).Should().BeSameAs( m );
        }

        [Test]
        public void test_Combined_traits_parsing()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m = c.FindOrCreate( "Beta+Alpha" );
            m.IsAtomic.Should().BeFalse();
            m.AtomicTraits.Should().HaveCount( 2, "Combined trait." );
            m.AtomicTraits[0].Should().BeSameAs( c.FindOrCreate( "Alpha" ), "Atomic Alpha is the first one." );
            m.AtomicTraits[1].Should().BeSameAs( c.FindOrCreate( "Beta" ), "Atomic Beta is the second one." );

            c.FindOrCreate( "Alpha+Beta" ).Should().BeSameAs( m, "Canonical order is ensured." );
            c.FindOrCreate( "+ +\t++ Alpha+++Beta++" ).Should().BeSameAs( m, "Extra characters and empty traits are ignored." );

            c.FindOrCreate( "Alpha+Beta+Alpha" ).Should().BeSameAs( m, "Multiple identical traits are removed." );
            c.FindOrCreate( "Alpha+ +Beta\t ++Beta+ + Alpha +    Beta   ++ " ).Should().BeSameAs( m, "Multiple identical traits are removed." );

            CKTrait m2 = c.FindOrCreate( "Beta+Alpha+Zeta+Tau+Pi+Omega+Epsilon" );
            c.FindOrCreate( "++Beta+Zeta+Omega+Epsilon+Alpha+Zeta+Epsilon+Zeta+Tau+Epsilon+Pi+Tau+Beta+Zeta+Omega+Beta+Pi+Alpha" ).Should().BeSameAs( m2, "Unicity of Atomic trait is ensured." );

            c.FindOnlyExisting( "Beta" ).ToString().Should().Be( "Beta" );
            c.FindOnlyExisting( "Beta+Gamma" ).ToString().Should().Be( "Beta" );
            c.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other" ).Should().BeSameAs( m );
            c.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi" ).ToString().Should().Be( "Alpha+Beta+Pi+Tau" );
        }

        [Test]
        public void test_FindOnlyExisting_with_its_optional_collector()
        {
            var c = ContextWithPlusSeparator();

            List<string> collector = new List<string>();
            c.FindOrCreate( "Beta+Alpha+Tau+Pi" );

            c.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi+Zeta", t => { collector.Add( t ); return true; } ).ToString().Should().Be( "Alpha+Beta+Pi+Tau" );
            String.Join( ",", collector ).Should().Be( "Gamma,Nimp,Other,Zeta" );

            collector.Clear();
            c.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi", t => { collector.Add( t ); return t != "Other"; } ).ToString().Should().Be( "Alpha+Beta" );
            String.Join( ",", collector ).Should().Be( "Gamma,Nimp,Other" );
        }

        [Test]
        public void test_Intersect_between_traits()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m1 = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = c.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            m1.Intersect( m2 ).ToString().Should().Be( "Combo+Fridge", "Works as expected :-)" );
            m2.Intersect( m1 ).Should().BeSameAs( m1.Intersect( m2 ), "Same object in both calls." );

            m2.Intersect( c.EmptyTrait ).Should().BeSameAs( c.EmptyTrait, "Intersecting empty gives empty." );
        }

        [Test]
        public void test_Union_of_traits()
        {
            var c = ContextWithPlusSeparator();
            CKTrait m1 = c.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = c.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            m1.Union( m2 ).ToString().Should().Be( "Alpha+Alt+Another+Beta+Combo+Fridge+Xtra", "Works as expected :-)" );
            m2.Union( m1 ).Should().BeSameAs( m1.Union( m2 ), "Same in both calls." );
        }

        [Test]
        public void test_Except_of_traits()
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
            m.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' since EmptyTrait.AtomicTraits.Count == 0..." );
            c.EmptyTrait.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' in itself." );
        }

        [Test]
        public void trait_separator_can_be_changed_from_the_default_pipe()
        {
            var c = new CKTraitContext( "SemiColonContext", ';' );
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
            m.Overlaps( c.EmptyTrait ).Should().BeFalse( "Empty is NOT contained 'ONE' since EmptyTrait.AtomicTraits.Count == 0..." );
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
        public void test_Fallbacks_generation()
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
        public void test_Fallbacks_ordering()
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
                f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ).Should().BeTrue( "CKTrait.CompareTo is ok." );
            }
            {
                CKTrait m = c.FindOrCreate( "xz+lz+ded+az+zer+t+zer+ce+ret+ert+ml+a+nzn" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToArray();
                f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ).Should().BeTrue( "CKTrait.CompareTo is ok." );
            }
        }

        [Test]
        public void test_FindIfAllExist()
        {
            var c = ContextWithPlusSeparator();

            CKTrait m = c.FindOrCreate( "Alpha+Beta+Combo+Fridge" );

            c.FindIfAllExist( "" ).Should().Be( c.EmptyTrait );
            c.FindIfAllExist( "bo" ).Should().BeNull();
            c.FindIfAllExist( "Alpha" ).Should().Be( c.FindOrCreate( "Alpha" ) );
            c.FindIfAllExist( "bo+pha" ).Should().BeNull();
            c.FindIfAllExist( "Fridge+Combo+Alpha+Beta" ).Should().BeSameAs( m );
        }
    }
}
