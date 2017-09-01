using FluentAssertions;
using System.IO;
using NUnit.Framework;

namespace CK.Core.Tests
{

    public class TemporaryFileTests
    {
        [Test]
        public void TemporaryFile_has_FileAttributes_Temporary_by_default()
        {
            string path = string.Empty;
            using (TemporaryFile temporaryFile = new TemporaryFile(true, null))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();

            using (TemporaryFile temporaryFile = new TemporaryFile())
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();

            using (TemporaryFile temporaryFile = new TemporaryFile(true))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();
        }

        [Test]
        public void an_empty_extension_is_like_no_extension()
        {
            string path = string.Empty;
            using (TemporaryFile temporaryFile = new TemporaryFile(" "))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();
        }

        [Test]
        public void TemporaryFileExtensionTest()
        {
            string path = string.Empty;
            using (TemporaryFile temporaryFile = new TemporaryFile(" "))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();

            using (TemporaryFile temporaryFile = new TemporaryFile(true, "."))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                path.EndsWith(".tmp.").Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();

            using (TemporaryFile temporaryFile = new TemporaryFile(true, "tst"))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                path.EndsWith(".tmp.tst").Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();

            using (TemporaryFile temporaryFile = new TemporaryFile(true, ".tst"))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                path.EndsWith(".tmp.tst").Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
            }
            File.Exists(path).Should().BeFalse();
        }

        [Test]
        public void TemporaryFileDetachTest()
        {
            string path = string.Empty;
            using (TemporaryFile temporaryFile = new TemporaryFile(true, null))
            {
                path = temporaryFile.Path;
                File.Exists(temporaryFile.Path).Should().BeTrue();
                (File.GetAttributes(temporaryFile.Path) & FileAttributes.Temporary).Should().Be(FileAttributes.Temporary);
                temporaryFile.Detach();
            }
            File.Exists(path).Should().BeTrue();
            File.Delete(path);
        }
    }
}
