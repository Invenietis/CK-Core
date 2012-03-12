#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.MultiMode.Tests\Modes.cs) is part of CiviKey. 
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
    /// This class test operations on MultiMode (ObtainMode, Intersect, etc.).
    /// </summary>
    [TestFixture]
    public class Modes
    {
        MultiModeContext Context;

        [SetUp]
        public void Setup()
        {
            Context = new MultiModeContext();
        }

        [Test]
        public void EmptyOne()
        {
            MultiMode m = Context.EmptyMode;
            Assert.That( m.ToString() == String.Empty, "Empty mode is the empty string." );
            Assert.That( m.IsAtomic, "Empty mode is considered as atomic." );
            Assert.That( m.AtomicModes.Count == 0, "Empty mode has no atomic modes inside." );

            Assert.That( Context.ObtainMode( "" ) == m, "Obtaining empty string gives the empty mode." );
            Assert.That( Context.ObtainMode( "+" ) == m, "Obtaining '+' gives the empty mode." );
            Assert.That( Context.ObtainMode( " \t \r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.ObtainMode( "+ \t +" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.ObtainMode( "++++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.ObtainMode( "++  +++ \r\n  + \t +" ) == m, "Multiple empty strings leads to empty mode." );

        }

        [Test]
        public void OneAtomicMode()
        {
            MultiMode m = Context.ObtainMode( "Alpha" );
            Assert.That( m.IsAtomic && m.AtomicModes.Count == 1, "Not a combined one." );
            Assert.That( m.AtomicModes[0] == m, "Atomic modes are self-contained." );

            Assert.That( Context.ObtainMode( " \t Alpha\r\n  " ) == m, "Strings are trimmed." );
            Assert.That( Context.ObtainMode( "+ \t Alpha+" ) == m, "Leading and trailing '+' are ignored." );
            Assert.That( Context.ObtainMode( "+Alpha+++" ) == m, "Multiple + are ignored" );
            Assert.That( Context.ObtainMode( "++ Alpha +++ \r\n  + \t +" ) == m, "Multiple empty strings are ignored." );
        }

        [Test]
        public void CombinedModes()
        {
            MultiMode m = Context.ObtainMode( "Beta+Alpha" );
            Assert.That( !m.IsAtomic && m.AtomicModes.Count == 2, "Combined mode." );
            Assert.That( m.AtomicModes[0] == Context.ObtainMode( "Alpha" ), "Atomic Alpha is the first one." );
            Assert.That( m.AtomicModes[1] == Context.ObtainMode( "Beta" ), "Atomic Beta is the second one." );

            Assert.That( Context.ObtainMode( "Alpha+Beta" ) == m, "Canonical order is ensured." );
            Assert.That( Context.ObtainMode( "+ +\t++ Alpha+++Beta++" ) == m, "Extra characters and empty modes are ignored." );

            Assert.That( Context.ObtainMode( "Alpha+Beta+Alpha" ) == m, "Multiple identical modes are removed." );
            Assert.That( Context.ObtainMode( "Alpha+ +Beta\r++Beta+ + Alpha +    Beta   ++ " ) == m, "Multiple identical modes are removed." );

            m = Context.ObtainMode( "Beta+Alpha+Zeta+Tau+Pi+Omega+Epsilon" );
            Assert.That( Context.ObtainMode( "++Beta+Zeta+Omega+Epsilon+Alpha+Zeta+Epsilon+Zeta+Tau+Epsilon+Pi+Tau+Beta+Zeta+Omega+Beta+Pi+Alpha" ) == m,
                "Unicity of Atomic mode is ensured." );
        }

        [Test]
        public void IntersectModes()
        {
            MultiMode m1 = Context.ObtainMode( "Beta+Alpha+Fridge+Combo" );
            MultiMode m2 = Context.ObtainMode( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Intersect( m2 ).ToString() == "Combo+Fridge", "Works as expected :-)" );
            Assert.That( m2.Intersect( m1 ) == m1.Intersect( m2 ), "Same object in both calls." );

            Assert.That( m2.Intersect( Context.EmptyMode ) == Context.EmptyMode, "Intersecting empty gives empty." );
        }

        [Test]
        public void AddModes()
        {
            MultiMode m1 = Context.ObtainMode( "Beta+Alpha+Fridge+Combo" );
            MultiMode m2 = Context.ObtainMode( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Add( m2 ).ToString() == "Alpha+Alt+Another+Beta+Combo+Fridge+Xtra", "Works as expected :-)" );
            Assert.That( m2.Add( m1 ) == m1.Add( m2 ), "Same in both calls." );
        }

        [Test]
        public void RemoveModes()
        {
            MultiMode m1 = Context.ObtainMode( "Beta+Alpha+Fridge+Combo" );
            MultiMode m2 = Context.ObtainMode( "Xtra+Combo+Another+Fridge+Alt" );

            Assert.That( m1.Remove( m2 ).ToString() == "Alpha+Beta", "Works as expected :-)" );
            Assert.That( m2.Remove( m1 ).ToString() == "Alt+Another+Xtra", "Works as expected..." );

            Assert.That( m2.Remove( Context.EmptyMode ) == m2 && m1.Remove( Context.EmptyMode ) == m1, "Removing empty does nothing." );
        }


        [Test]
        public void ContainsModes()
        {
            MultiMode m = Context.ObtainMode( "Beta+Alpha+Fridge+Combo" );

            Assert.That( Context.EmptyMode.ContainsAll( Context.EmptyMode ), "Empty is contained by definition in itself." );
            Assert.That( m.ContainsAll( Context.EmptyMode ), "Empty is contained by definition." );
            Assert.That( m.ContainsAll( Context.ObtainMode( "Fridge+Alpha" ) ) );
            Assert.That( m.ContainsAll( Context.ObtainMode( "Fridge" ) ) );
            Assert.That( m.ContainsAll( Context.ObtainMode( "Fridge+Alpha+Combo" ) ) );
            Assert.That( m.ContainsAll( Context.ObtainMode( "Fridge+Alpha+Beta+Combo" ) ) );
            Assert.That( !m.ContainsAll( Context.ObtainMode( "Fridge+Lol" ) ) );
            Assert.That( !m.ContainsAll( Context.ObtainMode( "Murfn" ) ) );
            Assert.That( !m.ContainsAll( Context.ObtainMode( "Fridge+Alpha+Combo+Lol" ) ) );
            Assert.That( !m.ContainsAll( Context.ObtainMode( "Lol+Fridge+Alpha+Beta+Combo" ) ) );

            Assert.That( m.ContainsOne( Context.ObtainMode( "Fridge+Alpha" ) ) );
            Assert.That( m.ContainsOne( Context.ObtainMode( "Nimp+Fridge+Mourfn" ) ) );
            Assert.That( m.ContainsOne( Context.ObtainMode( "Fridge+Alpha+Combo+Albert" ) ) );
            Assert.That( m.ContainsOne( Context.ObtainMode( "ZZF+AAlp+BBeBe+Combo" ) ) );
            Assert.That( !m.ContainsOne( Context.ObtainMode( "AFridge+ALol" ) ) );
            Assert.That( !m.ContainsOne( Context.ObtainMode( "Murfn" ) ) );
            Assert.That( !m.ContainsOne( Context.ObtainMode( "QF+QA+QC+QL" ) ) );
            Assert.That( !m.ContainsOne( Context.EmptyMode ), "Empty is NOT contained 'ONE' since EmptyMode.AtomicModes.Count == 0..." );
            Assert.That( !Context.EmptyMode.ContainsOne( Context.EmptyMode ), "Empty is NOT contained 'ONE' in itself." );

        }

        [Test]
        public void ToggleModes()
        {
            MultiMode m = Context.ObtainMode( "Beta+Alpha+Fridge+Combo" );
            Assert.That( m.Toggle( Context.ObtainMode( "Beta" ) ).ToString() == "Alpha+Combo+Fridge" );
            Assert.That( m.Toggle( Context.ObtainMode( "Fridge+Combo" ) ).ToString() == "Alpha+Beta" );
            Assert.That( m.Toggle( Context.ObtainMode( "Beta+Fridge+Combo" ) ).ToString() == "Alpha" );
            Assert.That( m.Toggle( Context.ObtainMode( "Beta+Fridge+Combo+Alpha" ) ).ToString() == "" );

            Assert.That( m.Toggle( Context.ObtainMode( "" ) ).ToString() == "Alpha+Beta+Combo+Fridge" );
            Assert.That( m.Toggle( Context.ObtainMode( "Xtra" ) ).ToString() == "Alpha+Beta+Combo+Fridge+Xtra" );
            Assert.That( m.Toggle( Context.ObtainMode( "Alpha+Xtra" ) ).ToString() == "Beta+Combo+Fridge+Xtra" );
            Assert.That( m.Toggle( Context.ObtainMode( "Zenon+Alpha+Xtra+Fridge" ) ).ToString() == "Beta+Combo+Xtra+Zenon" );
        }


        [Test]
        public void Fallbacks()
        {
            {
                MultiMode m = Context.ObtainMode( "" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                MultiMode m = Context.ObtainMode( "Alpha" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
                Assert.That( f.Count == 1 );
                Assert.That( f[0].ToString() == "" );
            }
            {
                MultiMode m = Context.ObtainMode( "Alpha+Beta" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
                Assert.That( f.Count == 3 );
                Assert.That( f[0].ToString() == "Alpha" );
                Assert.That( f[1].ToString() == "Beta" );
                Assert.That( f[2].ToString() == "" );
            }
            {
                MultiMode m = Context.ObtainMode( "Alpha+Beta+Combo" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
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
                MultiMode m = Context.ObtainMode( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
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
                MultiMode m = Context.ObtainMode( "Alpha+Beta+Combo+Fridge" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;

                MultiMode[] sorted = f.ToArray();
                Array.Sort( sorted );
                Array.Reverse( sorted );
                Assert.That( sorted.SequenceEqual( f ), "KeyboardMode.CompareTo respects the fallbacks (fallbacks is in reverse order)." );
            }
            {
                MultiMode m = Context.ObtainMode( "Alpha+Beta+Combo+Fridge+F+K+Ju+J+A+B" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
                Assert.That( f.OrderBy( mode => mode ).Reverse().SequenceEqual( f ), "KeyboardMode.CompareTo is ok, thanks to Linq ;-)." );
            }
            {
                MultiMode m = Context.ObtainMode( "xz+lz+ded+az+zer+t+zer+ce+ret+ert+ml+a+nzn" );
                IReadOnlyList<MultiMode> f = m.Fallbacks;
                Assert.That( f.OrderBy( mode => mode ).Reverse().SequenceEqual( f ), "KeyboardMode.CompareTo is ok, thanks to Linq ;-)." );
            }
        }
    }
}
