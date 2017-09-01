using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;
using FluentAssertions;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                if (_testFolder == null) InitializePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if (_solutionFolder == null) InitializePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            DeleteFolder(TestFolder, true);
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

        static void InitializePaths()
        {
            _solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(GetTestProjectPath()));
            _testFolder = Path.Combine(_solutionFolder, "Tests", "CK.Core.Tests", "TestDir");
            Console.WriteLine($"SolutionFolder is: {_solutionFolder}.");
            Console.WriteLine($"TestFolder is: {_testFolder}.");
            Console.WriteLine($"Core path: {typeof(string).GetTypeInfo().Assembly.CodeBase}.");
            CleanupTestFolder();
        }

        static string GetTestProjectPath([CallerFilePath]string path = null) => Path.GetDirectoryName(path);

    }
}
