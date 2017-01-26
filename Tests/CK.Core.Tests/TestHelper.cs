using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using Xunit;
using FluentAssertions;

namespace CK.Core.Tests
{

    public static class Should
    {
        public static void Throw<T>(Action a) where T : Exception => a.ShouldThrow<T>();
        public static void Throw<T>(Action a, string because) where T : Exception => a.ShouldThrow<T>(because);
    }

#if !NET451
    class ExcludeFromCodeCoverageAttribute : Attribute
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
                if (_testFolder == null) InitalizePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if (_solutionFolder == null) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            DeleteFolder(TestFolder, true);
        }

        static public void ForceGCFullCollect()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        }

        public static void DeleteFolder(string directoryPath, bool recreate = false)
        {
            int tryCount = 0;
            for (;;)
            {
                try
                {
                    if (Directory.Exists(directoryPath)) Directory.Delete(directoryPath, true);
                    if (recreate)
                    {
                        Directory.CreateDirectory(directoryPath);
                        File.WriteAllText(Path.Combine(directoryPath, "TestWrite.txt"), "Test write works.");
                        File.Delete(Path.Combine(directoryPath, "TestWrite.txt"));
                    }
                    return;
                }
                catch (Exception ex)
                {
                    if (++tryCount == 20) throw;
                    Console.WriteLine("{1} - While cleaning up directory '{0}'. Retrying.", directoryPath, ex.Message);
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        static void InitalizePaths()
        {
#if NET451
            string p = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            p = Path.GetDirectoryName(p);
#else
            string p = Directory.GetCurrentDirectory();
#endif
            while (!File.Exists(Path.Combine(p, "CK-Core.sln")))
            {
                p = Path.GetDirectoryName(p);
            }
            _solutionFolder = p;
            _testFolder = Path.Combine(p, "Tests", "CK.Core.Tests", "TestDir");

            Console.WriteLine( $"SolutionFolder is: {_solutionFolder}\r\nTestFolder is: {_testFolder}" );
            CleanupTestFolder();
        }
    }
}
