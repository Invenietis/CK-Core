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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Linq;
using NUnit.Framework;
using CK.Core;
using System.Collections.Generic;

namespace Keyboard
{

    /// <summary>
    /// This class test operations on CKTrait (FindOrCreate, Intersect, etc.).
    /// </summary>
    [TestFixture]
    [Category( "CKTrait" )]
    public class Traits
    {
        CKTraitContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new CKTraitContext( "Test", '+' );
        }

        [Test]
        public void ComparingTraits()
        {
            CKTraitContext c1 = new CKTraitContext( "C1" );
            CKTraitContext c1Bis = new CKTraitContext( "C1" );
            CKTraitContext c2 = new CKTraitContext( "C2" );

            Assert.That( c1.CompareTo( c1 ), Is.EqualTo( 0 ) );
            Assert.That( c1.CompareTo( c2 ), Is.LessThan( 0 ) );
            Assert.That( c1Bis.CompareTo( c1 ), Is.GreaterThan( 0 ) );

            var tAc1 = c1.FindOrCreate( "A" );
            var tBc1 = c1.FindOrCreate( "B" );
            var tABc1 = c1.FindOrCreate( "A|B" );
            var tAc2 = c2.FindOrCreate( "A" );

            Assert.That( tAc1.CompareTo( tAc1 ), Is.EqualTo( 0 ) );
            Assert.That( tAc1.CompareTo( tBc1 ), Is.GreaterThan( 0 ), "In the same context, A is stronger than B." );
            Assert.That( tABc1.CompareTo( tBc1 ), Is.GreaterThan( 0 ), "In the same context, A|B is stronger than B." );
            Assert.That( tAc1.CompareTo( tAc2 ), Is.LessThan( 0 ), "Between different contexts, the context ordering drives the ordering." );
            Assert.That( tABc1.CompareTo( tAc2 ), Is.LessThan( 0 ), "Between different contexts, the context ordering drives the ordering." );
        }

        [Test]
        public void ContextMismatchOrNull()
        {
            Assert.Throws<ArgumentException>( () => new CKTraitContext( null ) );
            Assert.Throws<ArgumentException>( () => new CKTraitContext( "  " ) );

            CKTraitContext c1 = new CKTraitContext( "C1" );
            CKTraitContext c2 = new CKTraitContext( "C2" );

            var t1 = c1.FindOrCreate( "T1" );
            var t2 = c2.FindOrCreate( "T2" );
            Assert.That( t1 != t2 );
            Assert.Throws<InvalidOperationException>( () => t1.Union( t2 ) );
            Assert.Throws<InvalidOperationException>( () => t1.Intersect( t2 ) );
            Assert.Throws<InvalidOperationException>( () => t1.Except( t2 ) );
            Assert.Throws<InvalidOperationException>( () => t1.SymmetricExcept( t2 ) );

            Assert.Throws<InvalidOperationException>( () => t1.Overlaps( t2 ) );
            Assert.Throws<InvalidOperationException>( () => t1.IsSupersetOf( t2 ) );

            Assert.Throws<ArgumentNullException>( () => t1.Union( null ) );
            Assert.Throws<ArgumentNullException>( () => t1.Intersect( null ) );
            Assert.Throws<ArgumentNullException>( () => t1.Except( null ) );
            Assert.Throws<ArgumentNullException>( () => t1.SymmetricExcept( null ) );

            Assert.Throws<ArgumentNullException>( () => t1.Overlaps( null ) );
            Assert.Throws<ArgumentNullException>( () => t1.IsSupersetOf( null ) );

            Assert.Throws<ArgumentNullException>( () => t1.CompareTo( null ) );
            Assert.Throws<ArgumentNullException>( () => c1.CompareTo( null ) );
        }

        [Test]
        public void EmptyOne()
        {
            CKTrait m = Context.EmptyTrait;
            Assert.That( m.ToString() == String.Empty, "Empty trait is the empty string." );
            Assert.That( m.IsAtomic, "Empty trait is considered as atomic." );
            Assert.That( m.AtomicTraits.Count == 0, "Empty trait has no atomic traits inside." );

            Assert.That( Context.FindOrCreate( (string)null ) == m, "Null gives the empty trait." );
            Assert.That( Context.FindOrCreate( "" ) == m, "Obtaining empty string gives the empty trait." );
            Assert.That( Context.FindOrCreate( "+" ) == m, "Obtaining '+' gives the empty trait." );
            Assert.That( Context.FindOrCreate( " \t \r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.FindOrCreate( "+ \t +" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.FindOrCreate( "++++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.FindOrCreate( "++  +++ \r\n  + \t +" ) == m, "Multiple empty strings leads to empty trait." );

            Assert.That( Context.FindOnlyExisting( null ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "" ), Is.Null );
            Assert.That( Context.FindOnlyExisting( " " ), Is.Null );
            Assert.That( Context.FindOnlyExisting( " ++  + " ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "NONE" ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "NO+NE" ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "N+O+N+E" ), Is.Null );
        }

        [Test]
        public void OneAtomicTrait()
        {
            CKTrait m = Context.FindOrCreate( "Alpha" );
            Assert.That( m.IsAtomic && m.AtomicTraits.Count == 1, "Not a combined one." );
            Assert.That( m.AtomicTraits[0] == m, "Atomic traits are self-contained." );

            Assert.That( Context.FindOrCreate( " \t Alpha\r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.FindOrCreate( "+ \t Alpha+" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.FindOrCreate( "+Alpha+++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.FindOrCreate( "++ Alpha +++ \r\n  + \t +" ) == m, "Multiple empty strings are ignored." );

            Assert.That( Context.FindOnlyExisting( "Beta" ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma" ), Is.Null );
            Assert.That( Context.FindOnlyExisting( "Alpha" ), Is.SameAs( m ) );
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma+Alpha" ), Is.SameAs( m ) );
        }

        [Test]
        public void CombinedTraits()
        {
            CKTrait m = Context.FindOrCreate( "Beta+Alpha" );
            Assert.That( !m.IsAtomic && m.AtomicTraits.Count == 2, "Combined trait." );
            Assert.That( m.AtomicTraits[0] == Context.FindOrCreate( "Alpha" ), "Atomic Alpha is the first one." );
            Assert.That( m.AtomicTraits[1] == Context.FindOrCreate( "Beta" ), "Atomic Beta is the second one." );

            Assert.That( Context.FindOrCreate( "Alpha+Beta" ) == m, "Canonical order is ensured." );
            Assert.That( Context.FindOrCreate( "+ +\t++ Alpha+++Beta++" ) == m, "Extra characters and empty traits are ignored." );

            Assert.That( Context.FindOrCreate( "Alpha+Beta+Alpha" ) == m, "Multiple identical traits are removed." );
            Assert.That( Context.FindOrCreate( "Alpha+ +Beta\r++Beta+ + Alpha +    Beta   ++ " ) == m, "Multiple identical traits are removed." );

            CKTrait m2 = Context.FindOrCreate( "Beta+Alpha+Zeta+Tau+Pi+Omega+Epsilon" );
            Assert.That( Context.FindOrCreate( "++Beta+Zeta+Omega+Epsilon+Alpha+Zeta+Epsilon+Zeta+Tau+Epsilon+Pi+Tau+Beta+Zeta+Omega+Beta+Pi+Alpha" ), Is.SameAs( m2 ), "Unicity of Atomic trait is ensured." );

            Assert.That( Context.FindOnlyExisting( "Beta" ).ToString(), Is.EqualTo( "Beta" ) );
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma" ).ToString(), Is.EqualTo( "Beta" ) );
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other" ), Is.SameAs( m ) );
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi" ).ToString(), Is.EqualTo( "Alpha+Beta+Pi+Tau" ) );
        }

        [Test]
        public void FindOnlyExistingCollector()
        {
            List<string> collector = new List<string>();
            Context.FindOrCreate( "Beta+Alpha+Tau+Pi" );

            Assert.That( Context.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi+Zeta", t => { collector.Add( t ); return true; } ).ToString(), Is.EqualTo( "Alpha+Beta+Pi+Tau" ) );
            Assert.That( String.Join( ",", collector ), Is.EqualTo( "Gamma,Nimp,Other,Zeta" ) );

            collector.Clear();
            Assert.That( Context.FindOnlyExisting( "Beta+Gamma+Nimp+Alpha+Other+Tau+Pi", t => { collector.Add( t ); return t != "Other"; } ).ToString(), Is.EqualTo( "Alpha+Beta" ) );
            Assert.That( String.Join( ",", collector ), Is.EqualTo( "Gamma,Nimp,Other" ) );
        }

        [Test]
        public void IntersectTraits()
        {
            CKTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Intersect( m2 ).ToString() == "Combo+Fridge", "Works as expected :-)" );
            Assert.That( m2.Intersect( m1 ) == m1.Intersect( m2 ), "Same object in both calls." );

            Assert.That( m2.Intersect( Context.EmptyTrait ) == Context.EmptyTrait, "Intersecting empty gives empty." );
        }

        [Test]
        public void AddTraits()
        {
            CKTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Union( m2 ).ToString() == "Alpha+Alt+Another+Beta+Combo+Fridge+Xtra", "Works as expected :-)" );
            Assert.That( m2.Union( m1 ) == m1.Union( m2 ), "Same in both calls." );
        }

        [Test]
        public void RemoveTraits()
        {
            CKTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            CKTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Except( m2 ).ToString() == "Alpha+Beta", "Works as expected :-)" );
            Assert.That( m2.Except( m1 ).ToString() == "Alt+Another+Xtra", "Works as expected..." );

            Assert.That( m2.Except( Context.EmptyTrait ) == m2 && m1.Except( Context.EmptyTrait ) == m1, "Removing empty does nothing." );
        }


        [Test]
        public void ContainsTraits()
        {
            CKTrait m = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );

            Assert.That( Context.EmptyTrait.IsSupersetOf( Context.EmptyTrait ), "Empty is contained by definition in itself." );
            Assert.That( m.IsSupersetOf( Context.EmptyTrait ), "Empty is contained by definition." );
            Assert.That( m.IsSupersetOf( Context.FindOrCreate( "Fridge+Alpha" ) ) );
            Assert.That( m.IsSupersetOf( Context.FindOrCreate( "Fridge" ) ) );
            Assert.That( m.IsSupersetOf( Context.FindOrCreate( "Fridge+Alpha+Combo" ) ) );
            Assert.That( m.IsSupersetOf( Context.FindOrCreate( "Fridge+Alpha+Beta+Combo" ) ) );
            Assert.That( !m.IsSupersetOf( Context.FindOrCreate( "Fridge+Lol" ) ) );
            Assert.That( !m.IsSupersetOf( Context.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.IsSupersetOf( Context.FindOrCreate( "Fridge+Alpha+Combo+Lol" ) ) );
            Assert.That( !m.IsSupersetOf( Context.FindOrCreate( "Lol+Fridge+Alpha+Beta+Combo" ) ) );

            Assert.That( m.Overlaps( Context.FindOrCreate( "Fridge+Alpha" ) ) );
            Assert.That( m.Overlaps( Context.FindOrCreate( "Nimp+Fridge+Mourfn" ) ) );
            Assert.That( m.Overlaps( Context.FindOrCreate( "Fridge+Alpha+Combo+Albert" ) ) );
            Assert.That( m.Overlaps( Context.FindOrCreate( "ZZF+AAlp+BBeBe+Combo" ) ) );
            Assert.That( !m.Overlaps( Context.FindOrCreate( "AFridge+ALol" ) ) );
            Assert.That( !m.Overlaps( Context.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.Overlaps( Context.FindOrCreate( "QF+QA+QC+QL" ) ) );
            Assert.That( !m.Overlaps( Context.EmptyTrait ), "Empty is NOT contained 'ONE' since EmptyTrait.AtomicTraits.Count == 0..." );
            Assert.That( !Context.EmptyTrait.Overlaps( Context.EmptyTrait ), "Empty is NOT contained 'ONE' in itself." );

        }

        [Test]
        public void PipeDefaultTrait()
        {
            var c = new CKTraitContext( "PipeContext", '|' );
            CKTrait m = c.FindOrCreate( "Beta|Alpha|Fridge|Combo" );

            Assert.That( c.EmptyTrait.IsSupersetOf( c.EmptyTrait ), "Empty is contained by definition in itself." );
            Assert.That( m.IsSupersetOf( c.EmptyTrait ), "Empty is contained by definition." );
            Assert.That( m.IsSupersetOf( c.FindOrCreate( "Fridge|Alpha" ) ) );
            Assert.That( m.IsSupersetOf( c.FindOrCreate( "Fridge" ) ) );
            Assert.That( m.IsSupersetOf( c.FindOrCreate( "Fridge|Alpha|Combo" ) ) );
            Assert.That( m.IsSupersetOf( c.FindOrCreate( "Fridge|Alpha|Beta|Combo" ) ) );
            Assert.That( !m.IsSupersetOf( c.FindOrCreate( "Fridge|Lol" ) ) );
            Assert.That( !m.IsSupersetOf( c.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.IsSupersetOf( c.FindOrCreate( "Fridge|Alpha|Combo+Lol" ) ) );
            Assert.That( !m.IsSupersetOf( c.FindOrCreate( "Lol|Fridge|Alpha|Beta|Combo" ) ) );

            Assert.That( m.Overlaps( c.FindOrCreate( "Fridge|Alpha" ) ) );
            Assert.That( m.Overlaps( c.FindOrCreate( "Nimp|Fridge|Mourfn" ) ) );
            Assert.That( m.Overlaps( c.FindOrCreate( "Fridge|Alpha|Combo|Albert" ) ) );
            Assert.That( m.Overlaps( c.FindOrCreate( "ZZF|AAlp|BBeBe|Combo" ) ) );
            Assert.That( !m.Overlaps( c.FindOrCreate( "AFridge|ALol" ) ) );
            Assert.That( !m.Overlaps( c.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.Overlaps( c.FindOrCreate( "QF|QA|QC|QL" ) ) );
            Assert.That( !m.Overlaps( c.EmptyTrait ), "Empty is NOT contained 'ONE' since EmptyTrait.AtomicTraits.Count == 0..." );
            Assert.That( !c.EmptyTrait.Overlaps( c.EmptyTrait ), "Empty is NOT contained 'ONE' in itself." );
        }

        [Test]
        public void ToggleTraits()
        {
            CKTrait m = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Beta" ) ).ToString() == "Alpha+Combo+Fridge" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Fridge+Combo" ) ).ToString() == "Alpha+Beta" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Beta+Fridge+Combo" ) ).ToString() == "Alpha" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Beta+Fridge+Combo+Alpha" ) ).ToString() == "" );

            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "" ) ).ToString() == "Alpha+Beta+Combo+Fridge" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Xtra" ) ).ToString() == "Alpha+Beta+Combo+Fridge+Xtra" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Alpha+Xtra" ) ).ToString() == "Beta+Combo+Fridge+Xtra" );
            Assert.That( m.SymmetricExcept( Context.FindOrCreate( "Zenon+Alpha+Xtra+Fridge" ) ).ToString() == "Beta+Combo+Xtra+Zenon" );
        }


        [Test]
        public void Fallbacks()
        {
            {
                CKTrait m = Context.FindOrCreate( "" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( m.FallbacksCount, Is.EqualTo( f.Count ) );
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                CKTrait m = Context.FindOrCreate( "Alpha" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( m.FallbacksCount, Is.EqualTo( f.Count ) );
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                CKTrait m = Context.FindOrCreate( "Alpha+Beta" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( m.FallbacksCount, Is.EqualTo( f.Count ) );
                Assert.That( f.Count == 3 );
                Assert.That( f[0].ToString() == "Alpha" );
                Assert.That( f[1].ToString() == "Beta" );
                Assert.That( f[2].ToString() == "" );
            }
            {
                CKTrait m = Context.FindOrCreate( "Alpha+Beta+Combo" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( m.FallbacksCount, Is.EqualTo( f.Count ) );
                Assert.That( f.Count == 7 );
                Assert.That( f[0].ToString() == "Alpha+Beta" );
                Assert.That( f[1].ToString() == "Alpha+Combo" );
                Assert.That( f[2].ToString() == "Beta+Combo" );
                Assert.That( f[3].ToString() == "Alpha" );
                Assert.That( f[4].ToString() == "Beta" );
                Assert.That( f[5].ToString() == "Combo" );
                Assert.That( f[6].ToString() == "" );
            }
            {
                CKTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( m.FallbacksCount, Is.EqualTo( f.Count ) );
                Assert.That( f.Count == 15 );
                Assert.That( f[0].ToString() == "Alpha+Beta+Combo" );
                Assert.That( f[1].ToString() == "Alpha+Beta+Fridge" );
                Assert.That( f[2].ToString() == "Alpha+Combo+Fridge" );
                Assert.That( f[3].ToString() == "Beta+Combo+Fridge" );
                Assert.That( f[4].ToString() == "Alpha+Beta" );
                Assert.That( f[5].ToString() == "Alpha+Combo" );
                Assert.That( f[6].ToString() == "Alpha+Fridge" );
                Assert.That( f[7].ToString() == "Beta+Combo" );
                Assert.That( f[8].ToString() == "Beta+Fridge" );
                Assert.That( f[9].ToString() == "Combo+Fridge" );
                Assert.That( f[10].ToString() == "Alpha" );
                Assert.That( f[11].ToString() == "Beta" );
                Assert.That( f[12].ToString() == "Combo" );
                Assert.That( f[13].ToString() == "Fridge" );
                Assert.That( f[14].ToString() == "" );
            }
        }

        [Test]
        public void FallbacksAndOrdering()
        {
            {
                CKTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();

                CKTrait[] sorted = f.ToArray();
                Array.Sort( sorted );
                Array.Reverse( sorted );
                Assert.That( sorted.SequenceEqual( f ), "KeyboardTrait.CompareTo respects the fallbacks (fallbacks is in reverse order)." );
            }
            {
                CKTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge+F+K+Ju+J+A+B" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ), "KeyboardTrait.CompareTo is ok, thanks to Linq ;-)." );
            }
            {
                CKTrait m = Context.FindOrCreate( "xz+lz+ded+az+zer+t+zer+ce+ret+ert+ml+a+nzn" );
                IReadOnlyList<CKTrait> f = m.Fallbacks.ToReadOnlyList();
                Assert.That( f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ), "KeyboardTrait.CompareTo is ok, thanks to Linq ;-)." );
            }
        }


        [Test]
        public void FindIfAllExist()
        {
            CKTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );

            Assert.That( Context.FindIfAllExist( "" ), Is.EqualTo( Context.EmptyTrait ) );
            Assert.That( Context.FindIfAllExist( "bo" ), Is.Null );
            Assert.That( Context.FindIfAllExist( "Alpha" ), Is.EqualTo( Context.FindOrCreate( "Alpha" ) ) );
            Assert.That( Context.FindIfAllExist( "bo+pha" ), Is.Null );
            Assert.That( Context.FindIfAllExist( "Fridge+Combo+Alpha+Beta" ), Is.SameAs( m ) );
        }
    }
}
