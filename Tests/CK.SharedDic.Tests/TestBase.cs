#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\TestBase.cs) is part of CiviKey. 
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

namespace SharedDic
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
                if (_appFolder == null) InitalizePaths();
                return _appFolder;
            }
        }

        public static string TestFolder
        {
            get
            {
                if (_testFolder == null) InitalizePaths();
                return _testFolder;
            }
        }

        public static string PluginFolder
        {
            get
            {
                if (_pluginFolder == null) InitalizePaths();
                return _pluginFolder;
            }
        }

        public static DirectoryInfo AppFolderDir
        {
            get { return _appFolderDir ?? (_appFolderDir = new DirectoryInfo(AppFolder)); }
        }

        public static DirectoryInfo TestFolderDir
        {
            get { return _testFolderDir ?? (_testFolderDir = new DirectoryInfo(TestFolder)); }
        }

        public static void CleanupTestDir()
        {
            if(TestFolderDir.Exists)
                TestFolderDir.Delete(true);
            TestFolderDir.Create();
        }

        public static void CopyPluginToTestDir(params FileInfo[] files)
        {
            if (_testFolder == null) InitalizePaths();
            foreach (FileInfo f in files)
            {
                File.Copy(f.FullName, Path.Combine(_testFolder, f.Name), true);
            }
        }

        public static void CopyPluginToTestDir(params string[] fileNames)
        {
            if (_testFolder == null) InitalizePaths();
            foreach (string f in fileNames)
            {
                File.Copy(Path.Combine(_pluginFolder, f), Path.Combine(_testFolder, f), true);
            }
        }

        private static void InitalizePaths()
        {
            string p = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            // Code base is like "file:///C:/Documents and Settings/Olivier Spinelli/Mes documents/Dev/CK/Output/Debug/App/CVKTests.DLL"
            StringAssert.StartsWith("file:///", p, "Code base must start with file:/// protocol.");

            p = p.Substring(8).Replace('/', System.IO.Path.DirectorySeparatorChar);

            // => Debug/
            p = Path.GetDirectoryName(p);
            _appFolder = p;

            // ==> Debug/SubTestDir
            _testFolder = Path.Combine(p, "SubTestDir");
            if (Directory.Exists(_testFolder)) Directory.Delete(_testFolder, true);
            Directory.CreateDirectory(_testFolder);

            // ==> Debug/Plugins
            _pluginFolder = Path.Combine(p, "Plugins");
        }

        static public string GetTestFilePath( string prefix, string name )
        {
            string path = Path.Combine( TestFolder, prefix + "." + name + ".xml" );
            if( !File.Exists( path ) )
            {
                FileStream fs = File.Create( path );
                fs.Dispose();
            }
            return path;
        }

        public static void CheckExactTypeAndValue( Type type, object value, object o )
        {
            Assert.That( o, Is.InstanceOf( type ) );
            Assert.AreEqual( value, o );
        }

        static public void DumpFileToConsole( string path )
        {
            Console.WriteLine( File.ReadAllText( path ) );
        }

    }
}
