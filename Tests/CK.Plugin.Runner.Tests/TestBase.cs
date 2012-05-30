#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Plugin.Runner.Tests\TestBase.cs) is part of CiviKey. 
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
using CK.Plugin.Hosting;
using System.Security;
using System.Threading;

namespace CK.Plugin.Runner
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
            TestFolderDir.Refresh();
            if( TestFolderDir.Exists ) TestFolderDir.Delete( true );
            TestFolderDir.Create();
        }

        public static void CopyPluginToTestDir( params FileInfo[] files )
        {
            if( _testFolder == null ) InitalizePaths();
            foreach( FileInfo f in files )
            {
                FileCopy( f.FullName, Path.Combine( _testFolder, f.Name ) );
            }
        }

        public static void CopyPluginToTestDir( params string[] fileNames )
        {
            if( _testFolder == null ) InitalizePaths();
            foreach( string f in fileNames )
            {
                FileCopy( Path.Combine( _pluginFolder, f ), Path.Combine( _testFolder, f ) );
            }
        }

        static void FileCopy( string source, string dest )
        {
            int retry = 0;
            while( retry < 3 )
            {
                try
                {
                    File.Copy( source, dest, true );
                    break;
                }
                catch( UnauthorizedAccessException )
                {
                    Thread.Sleep( 1 );
                    ++retry;
                }
                catch( DirectoryNotFoundException )
                {
                    Thread.Sleep( 1 );
                    ++retry;
                }
            }
        }

        private static void InitalizePaths()
        {
            string p = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            // Code base is like "file:///C:/Documents and Settings/Olivier Spinelli/Mes documents/Dev/CK/Output/Debug/App/CVKTests.DLL"
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
            _pluginFolder = Path.Combine( p, "Plugin.Runner.Tests.Plugins" );
        }


    }
}
