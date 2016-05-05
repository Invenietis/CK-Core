#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Monitoring.Tests\Persistence\MultiFileReaderTests.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Core;
using NUnit.Framework;

namespace CK.Monitoring.Tests.Persistence
{
    [TestFixture]
    public class CKMonPreviousVersionSupportTests
    {
        [SetUp]
        public void Setup()
        {
            TestHelper.InitalizePaths();
        }

        [Test]
        public void reading_old_ckmon_V0_files()
        {
            var folder = Path.Combine( TestHelper.SolutionFolder, "Tests/CK.Monitoring.Tests/Persistence/CKMon-v0/" );
            var files = Directory.GetFiles( folder, "*.ckmon", SearchOption.TopDirectoryOnly );
            MultiLogReader reader = new MultiLogReader();
            bool newIndex;
            for( int i = 0; i < files.Length; ++i )
            {
                var f = reader.Add( files[i], out newIndex );
                Assert.That( newIndex );
                Assert.That( f.Error, Is.Null );
            }
        }

    }
}
