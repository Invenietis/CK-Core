#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\ActionSequenceTests.cs) is part of CiviKey. 
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

using System.Reflection;
using CK.Core;
using NUnit.Framework;
using System.Linq;
using System;
using CK.Storage;

namespace Storage
{
    public class SpecEventArgs : EventArgs
    {
    }

    [TestFixture]
    [Category("StructuredStorage")]
    public class ActionSequenceTests
    {
        [Test]
        public void Test()
        {
            ActionSequence s = new ActionSequence();

            bool simpleCall = false;
            int oneParam = -1;
            string twoParam1 = null;
            DateTime twoParam2 = DateTime.MinValue;
            ActionSequenceTests threeParam1 = null;
            bool threeParam2 = false;
            double threeParam3 = 0;
            int addedCall = 0;

            s.Append(delegate { simpleCall = true; });
            s.Append(i => oneParam = i, 3712);
            s.Append((p1, p2) => { twoParam1 = p1; twoParam2 = p2; }, "p1 is set.", DateTime.Now);
            s.Append((p1, p2, p3) => { threeParam1 = p1; threeParam2 = p2; threeParam3 = p3; }, this, true, 0.34);
            s.Append(delegate
            {
                s.Append(delegate
                {
                    Assert.That(addedCall, Is.EqualTo(0));
                    ++addedCall;
                    s.Append(delegate
                    {
                        Assert.That(addedCall, Is.EqualTo(1));
                        ++addedCall;
                    });
                });
            });

            s.Run();

            Assert.That(simpleCall, Is.True);
            Assert.That(oneParam, Is.EqualTo(3712));
            Assert.That(twoParam1, Is.EqualTo("p1 is set."));
            Assert.That(twoParam2, Is.Not.EqualTo(DateTime.MinValue));

            Assert.That(threeParam1, Is.EqualTo(this));
            Assert.That(threeParam2, Is.EqualTo(true));
            Assert.That(threeParam3, Is.EqualTo(0.34));

            Assert.That(addedCall, Is.EqualTo(2));


        }


        [Test]
        public void TestNowOrLater()
        {
            ActionSequence s = new ActionSequence();

            int iStacked = 0;

            s.NowOrLater(x => { iStacked = x; }, 1);
            Assert.That(iStacked, Is.EqualTo(0), "ActionSequence s is not null nor read only: it is stacked.");

            s.ReadOnly = true;
            s.NowOrLater(x => { iStacked = x; }, 2);
            Assert.That(iStacked, Is.EqualTo(2), "ActionSequence s is read only: it is executed immediately.");

            s.Run();
            Assert.That(iStacked, Is.EqualTo(1), "First action has been deferred.");

            s = null;
            s.NowOrLater(x => { iStacked = x; }, 3);
            Assert.That(iStacked, Is.EqualTo(3), "This ActionSequence is null: it is executed immediately.");

        }


        public EventHandler StdEvent;
        public EventHandler<SpecEventArgs> SpecEvent;

        [Test]
        public void TestWithEvents()
        {
            int stdCalled = 0;
            int specCalled = 0;

            ActionSequence s = new ActionSequence();

            StdEvent += (o, e) => { stdCalled = 1; };
            SpecEvent += (o, e) => { specCalled = 1; };

            SpecEventArgs args = new SpecEventArgs();
            s.NowOrLater(SpecEvent, this, args);
            s.NowOrLater(StdEvent, this, EventArgs.Empty);

            Assert.That(stdCalled, Is.EqualTo(0));
            Assert.That(specCalled, Is.EqualTo(0));

            s.Run();
            Assert.That(stdCalled, Is.EqualTo(1));
            Assert.That(specCalled, Is.EqualTo(1));

        }
    }
}
