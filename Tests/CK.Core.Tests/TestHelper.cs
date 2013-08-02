#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\TestHelper.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [ExcludeFromCodeCoverage]
    static class TestHelper
    {
        static string _testFolder;
        static string _copyFolder;

        static DirectoryInfo _testFolderDir;
        static DirectoryInfo _copyFolderDir;
        
        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _console = new ActivityMonitorConsoleClient();
            _monitor = new ActivityMonitor();
            _monitor.Output.RegisterClients( _console );
        }

        public static IActivityMonitor ConsoleMonitor
        {
            get { return _monitor; }
        }

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value ) _monitor.Output.AtomicRegisterClient( c => c ==_console, () => _console );
                else _monitor.Output.UnregisterClient( _console );
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

        public static string CopyFolder
        {
            get
            {
                if( _copyFolder == null ) InitalizePaths();
                return _copyFolder;
            }
        }

        public static DirectoryInfo TestFolderDir
        {
            get { return _testFolderDir ?? (_testFolderDir = new DirectoryInfo( TestFolder )); }
        }

        public static DirectoryInfo CopyFolderDir
        {
            get { return _copyFolderDir ?? (_copyFolderDir = new DirectoryInfo( CopyFolder )); }
        }

        public static void CleanupTestDir()
        {
            if( TestFolderDir.Exists ) TestFolderDir.Delete( true );
            TestFolderDir.Create();
        }

        public static void CleanupCopyDir()
        {
            if( CopyFolderDir.Exists ) CopyFolderDir.Delete( true );
            CopyFolderDir.Create();
        }

        private static void InitalizePaths()
        {
            string p = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            // Code base is like "file:///C:/Documents and Settings/Olivier Spinelli/Mes documents/Dev/CK/Output/Debug/App/CVKTests.DLL"
            StringAssert.StartsWith( "file:///", p, "Code base must start with file:/// protocol." );

            p = p.Substring( 8 ).Replace( '/', System.IO.Path.DirectorySeparatorChar );

            // => Debug/
            p = Path.GetDirectoryName( p );

            // ==> Debug/SubTestDir
            _testFolder = Path.Combine( p, "SubTestDir" );
            if( Directory.Exists( _testFolder ) ) Directory.Delete( _testFolder, true );
            Directory.CreateDirectory( _testFolder );

            // ==> Debug/SubCopyTestDir
            _copyFolder = Path.Combine( p, "SubCopyTestDir" );
            if( Directory.Exists( _copyFolder ) ) Directory.Delete( _copyFolder, true );
            Directory.CreateDirectory( _copyFolder );

        }

    }
}
