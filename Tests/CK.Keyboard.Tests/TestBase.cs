using System.IO;
using NUnit.Framework;
using CK.Context;
using CK.Plugin.Config;
using System;

namespace Keyboard
{
    public static class TestBase
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
            if( TestFolderDir.Exists )
                TestFolderDir.Delete( true );
            TestFolderDir.Create();
        }

        public static void CopyPluginToTestDir( params FileInfo[] files )
        {
            if( _testFolder == null ) InitalizePaths();
            foreach( FileInfo f in files )
            {
                File.Copy( f.FullName, Path.Combine( _testFolder, f.Name ), true );
            }
        }

        public static void CopyPluginToTestDir( params string[] fileNames )
        {
            if( _testFolder == null ) InitalizePaths();
            foreach( string f in fileNames )
            {
                File.Copy( Path.Combine( _pluginFolder, f ), Path.Combine( _testFolder, f ), true );
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
            _pluginFolder = Path.Combine( p, "Plugins" );
        }

        /// <summary>
        /// Builds a valid path to a xml file used in this unit test.
        /// </summary>
        /// <param name="name">The file name (will be part of the resulting file name).</param>
        /// <returns>A valid full path.</returns>
        public static string GetTestFilePath( string name, bool createIfNotExists )
        {
            string path = Path.Combine( TestBase.TestFolder, "CKTests.Keyboard." + name + ".xml" );
            if( !File.Exists( path ) && createIfNotExists )
            {
                FileStream fs = File.Create( path );
                fs.Dispose();
            }
            return path;
        }

        public static string GetTestFilePath( string name )
        {
            return GetTestFilePath( name, true );
        }

        public static void DumpFileToConsole( string path )
        {
            Console.WriteLine( File.ReadAllText( path ) );
        }

        public static IContext CreateContext()
        {
            Host = new TestHost( "CKTests" ) { CustomSystemConfigPath = GetTestFilePath( "SystemConfiguration", false ) };
            return Host.CreateContext();
        }

        public static TestHost Host { get; set; }
    }

    public class TestHost : StandardContextHost
    {
        public TestHost( string appName )
            : base( appName, "2.5" )
        {
        }

        public new IObjectPluginConfig UserConfig { get { return base.UserConfig; } }

        public new IObjectPluginConfig SystemConfig { get { return base.SystemConfig; } }

        public string CustomSystemConfigPath { get; set; }

        public override string DefaultSystemConfigPath
        {
            get { return CustomSystemConfigPath; }
        }

        public new void SaveSystemConfig()
        {
            base.SaveSystemConfig();
        }

        public new void SaveUserConfig()
        {
            base.SaveUserConfig();
        }

        public new void SaveContext()
        {
            base.SaveContext();
        }
    }
}
