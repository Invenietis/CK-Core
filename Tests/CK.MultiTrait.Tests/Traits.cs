#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.MultiTrait.Tests\Traits.cs) is part of CiviKey. 
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

namespace Keyboard
{

    /// <summary>
    /// This class test operations on MultiTrait (FindOrCreate, Intersect, etc.).
    /// </summary>
    [TestFixture]
    public class Traits
    {
        MultiTraitContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new MultiTraitContext();
        }

        [Test]
        public void EmptyOne()
        {
            MultiTrait m = Context.EmptyTrait;
            Assert.That( m.ToString() == String.Empty, "Empty trait is the empty string." );
            Assert.That( m.IsAtomic, "Empty trait is considered as atomic." );
            Assert.That( m.AtomicTraits.Count == 0, "Empty trait has no atomic traits inside." );

            Assert.That( Context.FindOrCreate( "" ) == m, "Obtaining empty string gives the empty trait." );
            Assert.That( Context.FindOrCreate( "+" ) == m, "Obtaining '+' gives the empty trait." );
            Assert.That( Context.FindOrCreate( " \t \r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.FindOrCreate( "+ \t +" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.FindOrCreate( "++++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.FindOrCreate( "++  +++ \r\n  + \t +" ) == m, "Multiple empty strings leads to empty trait." );

        }

        [Test]
        public void OneAtomicTrait()
        {
            MultiTrait m = Context.FindOrCreate( "Alpha" );
            Assert.That( m.IsAtomic && m.AtomicTraits.Count == 1, "Not a combined one." );
            Assert.That( m.AtomicTraits[0] == m, "Atomic traits are self-contained." );

            Assert.That( Context.FindOrCreate( " \t Alpha\r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.FindOrCreate( "+ \t Alpha+" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.FindOrCreate( "+Alpha+++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.FindOrCreate( "++ Alpha +++ \r\n  + \t +" ) == m, "Multiple empty strings are ignored." );
        }

        [Test]
        public void CombinedTraits()
        {
            MultiTrait m = Context.FindOrCreate( "Beta+Alpha" );
            Assert.That( !m.IsAtomic && m.AtomicTraits.Count == 2, "Combined trait." );
            Assert.That( m.AtomicTraits[0] == Context.FindOrCreate( "Alpha" ), "Atomic Alpha is the first one." );
            Assert.That( m.AtomicTraits[1] == Context.FindOrCreate( "Beta" ), "Atomic Beta is the second one." );

            Assert.That( Context.FindOrCreate( "Alpha+Beta" ) == m, "Canonical order is ensured." );
            Assert.That( Context.FindOrCreate( "+ +\t++ Alpha+++Beta++" ) == m, "Extra characters and empty traits are ignored." );

            Assert.That( Context.FindOrCreate( "Alpha+Beta+Alpha" ) == m, "Multiple identical traits are removed." );
            Assert.That( Context.FindOrCreate( "Alpha+ +Beta\r++Beta+ + Alpha +    Beta   ++ " ) == m, "Multiple identical traits are removed." );

            m = Context.FindOrCreate( "Beta+Alpha+Zeta+Tau+Pi+Omega+Epsilon" );
            Assert.That( Context.FindOrCreate( "++Beta+Zeta+Omega+Epsilon+Alpha+Zeta+Epsilon+Zeta+Tau+Epsilon+Pi+Tau+Beta+Zeta+Omega+Beta+Pi+Alpha" ) == m,
                "Unicity of Atomic trait is ensured." );
        }

        [Test]
        public void IntersectTraits()
        {
            MultiTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            MultiTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Intersect( m2 ).ToString() == "Combo+Fridge", "Works as expected :-)" );
            Assert.That( m2.Intersect( m1 ) == m1.Intersect( m2 ), "Same object in both calls." );

            Assert.That( m2.Intersect( Context.EmptyTrait ) == Context.EmptyTrait, "Intersecting empty gives empty." );
        }

        [Test]
        public void AddTraits()
        {
            MultiTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            MultiTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Add( m2 ).ToString() == "Alpha+Alt+Another+Beta+Combo+Fridge+Xtra", "Works as expected :-)" );
            Assert.That( m2.Add( m1 ) == m1.Add( m2 ), "Same in both calls." );
        }

        [Test]
        public void RemoveTraits()
        {
            MultiTrait m1 = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            MultiTrait m2 = Context.FindOrCreate( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Remove( m2 ).ToString() == "Alpha+Beta", "Works as expected :-)" );
            Assert.That( m2.Remove( m1 ).ToString() == "Alt+Another+Xtra", "Works as expected..." );

            Assert.That( m2.Remove( Context.EmptyTrait ) == m2 && m1.Remove( Context.EmptyTrait ) == m1, "Removing empty does nothing." );
        }


        [Test]
        public void ContainsTraits()
        {
            MultiTrait m = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );

            Assert.That( Context.EmptyTrait.ContainsAll( Context.EmptyTrait ), "Empty is contained by definition in itself." );
            Assert.That( m.ContainsAll( Context.EmptyTrait ), "Empty is contained by definition." );
            Assert.That( m.ContainsAll( Context.FindOrCreate( "Fridge+Alpha" ) ) );
            Assert.That( m.ContainsAll( Context.FindOrCreate( "Fridge" ) ) );
            Assert.That( m.ContainsAll( Context.FindOrCreate( "Fridge+Alpha+Combo" ) ) );
            Assert.That( m.ContainsAll( Context.FindOrCreate( "Fridge+Alpha+Beta+Combo" ) ) );
            Assert.That( !m.ContainsAll( Context.FindOrCreate( "Fridge+Lol" ) ) );
            Assert.That( !m.ContainsAll( Context.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.ContainsAll( Context.FindOrCreate( "Fridge+Alpha+Combo+Lol" ) ) );
            Assert.That( !m.ContainsAll( Context.FindOrCreate( "Lol+Fridge+Alpha+Beta+Combo" ) ) );

            Assert.That( m.ContainsOne( Context.FindOrCreate( "Fridge+Alpha" ) ) );
            Assert.That( m.ContainsOne( Context.FindOrCreate( "Nimp+Fridge+Mourfn" ) ) );
            Assert.That( m.ContainsOne( Context.FindOrCreate( "Fridge+Alpha+Combo+Albert" ) ) );
            Assert.That( m.ContainsOne( Context.FindOrCreate( "ZZF+AAlp+BBeBe+Combo" ) ) );
            Assert.That( !m.ContainsOne( Context.FindOrCreate( "AFridge+ALol" ) ) );
            Assert.That( !m.ContainsOne( Context.FindOrCreate( "Murfn" ) ) );
            Assert.That( !m.ContainsOne( Context.FindOrCreate( "QF+QA+QC+QL" ) ) );
            Assert.That( !m.ContainsOne( Context.EmptyTrait ), "Empty is NOT contained 'ONE' since EmptyTrait.AtomicTraits.Count == 0..." );
            Assert.That( !Context.EmptyTrait.ContainsOne( Context.EmptyTrait ), "Empty is NOT contained 'ONE' in itself." );

        }

        [Test]
        public void ToggleTraits()
        {
            MultiTrait m = Context.FindOrCreate( "Beta+Alpha+Fridge+Combo" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Beta" ) ).ToString() == "Alpha+Combo+Fridge" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Fridge+Combo" ) ).ToString() == "Alpha+Beta" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Beta+Fridge+Combo" ) ).ToString() == "Alpha" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Beta+Fridge+Combo+Alpha" ) ).ToString() == "" );

            Assert.That( m.Toggle( Context.FindOrCreate( "" ) ).ToString() == "Alpha+Beta+Combo+Fridge" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Xtra" ) ).ToString() == "Alpha+Beta+Combo+Fridge+Xtra" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Alpha+Xtra" ) ).ToString() == "Beta+Combo+Fridge+Xtra" );
            Assert.That( m.Toggle( Context.FindOrCreate( "Zenon+Alpha+Xtra+Fridge" ) ).ToString() == "Beta+Combo+Xtra+Zenon" );
        }


        [Test]
        public void Fallbacks()
        {
            {
                MultiTrait m = Context.FindOrCreate( "" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                MultiTrait m = Context.FindOrCreate( "Alpha" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                MultiTrait m = Context.FindOrCreate( "Alpha+Beta" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
                Assert.That( f.Count == 3 );
                Assert.That( f[0].ToString() == "Alpha" );
                Assert.That( f[1].ToString() == "Beta" );
                Assert.That( f[2].ToString() == "" );
            }
            {
                MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
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
                MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
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
                MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;

                MultiTrait[] sorted = f.ToArray();
                Array.Sort( sorted );
                Array.Reverse( sorted );
                Assert.That( sorted.SequenceEqual( f ), "KeyboardTrait.CompareTo respects the fallbacks (fallbacks is in reverse order)." );
            }
            {
                MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge+F+K+Ju+J+A+B" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
                Assert.That( f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ), "KeyboardTrait.CompareTo is ok, thanks to Linq ;-)." );
            }
            {
                MultiTrait m = Context.FindOrCreate( "xz+lz+ded+az+zer+t+zer+ce+ret+ert+ml+a+nzn" );
                IReadOnlyList<MultiTrait> f = m.Fallbacks;
                Assert.That( f.OrderBy( trait => trait ).Reverse().SequenceEqual( f ), "KeyboardTrait.CompareTo is ok, thanks to Linq ;-)." );
            }
        }


        [Test]
        public void FindIfAllExist()
        {
            MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );

            Assert.That( Context.FindIfAllExist( "" ), Is.EqualTo( Context.EmptyTrait ) );
            Assert.That( Context.FindIfAllExist( "bo" ), Is.Null );
            Assert.That( Context.FindIfAllExist( "Alpha" ), Is.EqualTo( Context.FindOrCreate( "Alpha" ) ) );
            Assert.That( Context.FindIfAllExist( "bo+pha" ), Is.Null );
            Assert.That( Context.FindIfAllExist( "Fridge+Combo+Alpha+Beta" ), Is.SameAs( m ) );
        }

        [Test]
        public void FindClosest()
        {
            MultiTrait m = Context.FindOrCreate( "Alpha+Beta+Combo+Fridge" );

            Assert.That( ClosestString( m, "" ), Is.EqualTo( String.Empty ) );
            Assert.That( ClosestString( m, "POP+AZAZ", "LKL+ZEU" ), Is.EqualTo( null ) );
            Assert.That( ClosestString( m, "POP+AZAZ", "LKL+ZEU", "" ), Is.EqualTo( String.Empty ) );

            Assert.That( Context.FindIfAllExist( "POP" ), Is.Null, "No side effects if trait does not exist." );
            
            Assert.That( ClosestString( m, "Alpha+Beta", "Combo+Fridge", "Beta+Combo+Fridge", "Alpha+Beta+Combo+Fridge" ), Is.EqualTo( "Alpha+Beta+Combo+Fridge" ) );
            Assert.That( ClosestString( m, "Alpha+Beta", "Combo+Fridge", "Beta+Combo+Fridge" ), Is.EqualTo( "Beta+Combo+Fridge" ) );
            Assert.That( ClosestString( m, "Beta+Alpha+Fridge", "Combo+Fridge", "Beta+Combo+Fridge" ), Is.EqualTo( "Beta+Alpha+Fridge" ), "Since Alpha is better than Beta" );
            Assert.That( ClosestString( m, "Beta+Fridge", "Combo+Fridge", "Alpha" ), Is.EqualTo( "Beta+Fridge" ) );
        }

        string ClosestString( MultiTrait m, params string[] strings )
        {
            return m.Closest( strings, Util.FuncIdentity );
        }
    }
}
