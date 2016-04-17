using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace CK.Text.Tests
{
#if CSPROJ
    static class Does
    {
        public static SubstringConstraint Contain( string expected ) => Is.StringContaining( expected );

        public static EndsWithConstraint EndWith( string expected ) => Is.StringEnding( expected );

        public static StartsWithConstraint StartWith( string expected ) => Is.StringStarting( expected );

        public static ConstraintExpression Not => Is.Not;

        public static SubstringConstraint Contain( this ConstraintExpression @this, string expected ) => @this.StringContaining( expected );
    }
#else
    class TestAttribute : Xunit.FactAttribute
    {
    }
#endif

    static partial class TestHelper
    {
        static string _testFolder;
        static string _solutionFolder;
        
        static TestHelper()
        {
        }

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitalizePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            DeleteFolder( TestFolder, true );
        }

        static public void ForceGCFullCollect()
        {
            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced, true );
        }

        public static void DeleteFolder( string directoryPath, bool recreate = false )
        {
            int tryCount = 0;
            for( ; ; )
            {
                try
                {
                    if( Directory.Exists( directoryPath ) ) Directory.Delete( directoryPath, true );
                    if( recreate )
                    {
                        Directory.CreateDirectory( directoryPath );
                        File.WriteAllText( Path.Combine( directoryPath, "TestWrite.txt" ), "Test write works." );
                        File.Delete( Path.Combine( directoryPath, "TestWrite.txt" ) );
                    }
                    return;
                }
                catch( Exception ex )
                {
                    if( ++tryCount == 20 ) throw;
                    Console.WriteLine( "{1} - While cleaning up directory '{0}'. Retrying.", directoryPath, ex.Message );
                    System.Threading.Thread.Sleep( 100 );
                }
            }
        }

        static void InitalizePaths()
        {
#if NET451
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            p = Path.GetDirectoryName( Path.GetDirectoryName( Path.GetDirectoryName( p ) ) );
#else
            string p = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath;
#endif
            _testFolder = Path.Combine( p, "TestDir" );
            do
            {
                p = Path.GetDirectoryName( p );
            }
            while( !File.Exists( Path.Combine( p, "CK-Core.sln" ) ) );
            _solutionFolder = p;

            Console.WriteLine( "SolutionFolder is: {1}\r\nTestFolder is: {0}", _testFolder, _solutionFolder );
            CleanupTestFolder();
        }
    }
}
