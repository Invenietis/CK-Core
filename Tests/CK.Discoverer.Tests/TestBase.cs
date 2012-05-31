#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Discoverer.Tests\TestBase.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using System.Threading;

namespace Discoverer
{
    public class TestBase
    {
        static string _testFolder;
        static string _appFolder;
        static string _pluginFolder;

        static DirectoryInfo _testFolderDir;
        static DirectoryInfo _appFolderDir;

        public static string AppFolder
        {
            get
            {
                if( _appFolder == null ) InitalizePaths();
                return _appFolder;
            }
        }

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitalizePaths();
                return _testFolder;
            }
        }

        public static string PluginFolder
        {
            get
            {
                if( _pluginFolder == null ) InitalizePaths();
                return _pluginFolder;
            }
        }

        public static DirectoryInfo AppFolderDir
        {
            get { return _appFolderDir ?? (_appFolderDir = new DirectoryInfo( AppFolder )); }
        }

        public static DirectoryInfo TestFolderDir
        {
            get { return _testFolderDir ?? (_testFolderDir = new DirectoryInfo( TestFolder )); }
        }

        public static void CleanupTestDir()
        {
            if( _testFolder == null ) InitalizePaths();
            TestFolderDir.Refresh();
            if( TestFolderDir.Exists ) TestFolderDir.Delete( true );
            TestFolderDir.Create();
            TestFolderDir.Refresh();
        }

        public static void CopyPluginToTestDir( params string[] fileNames )
        {
            if( _testFolder == null ) InitalizePaths();
            TestFolderDir.Refresh();
            foreach( string f in fileNames )
            {
                string target = Path.Combine( _testFolder, f );
                Directory.CreateDirectory( Path.GetDirectoryName( target ) );
                File.Copy( Path.Combine( _pluginFolder, f ), target, true );
            }
            TestFolderDir.Refresh();
        }

        public static void RemovePluginFromTestDir( params string[] fileNames )
        {
            if( _testFolder == null ) InitalizePaths();
            TestFolderDir.Refresh();
            foreach( string f in fileNames )
            {
                File.Delete( Path.Combine( _testFolder, f ) );
            }
            TestFolderDir.Refresh();
        }

        private static void InitalizePaths()
        {
            string p = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            // Code base is like "file:///C:/Users/Spi/Documents/Dev4/CK-Core/Output/Tests/Debug/CK.Discoverer.Tests.DLL"
            StringAssert.StartsWith( "file:///", p, "Code base must start with file:/// protocol." );

            p = p.Substring( 8 ).Replace( '/', System.IO.Path.DirectorySeparatorChar );

            // => Debug/
            p = Path.GetDirectoryName( p );
            _appFolder = p;

            // ==> Debug/SubTestDir
            _testFolder = Path.Combine( p, "SubTestDir" );
            if( Directory.Exists( _testFolder ) ) Directory.Delete( _testFolder, true );
            Directory.CreateDirectory( _testFolder );

            // ==> Debug/Plugins
            _pluginFolder = Path.Combine( p, "Discoverer.Tests.Plugins" );
        }
    }
}
