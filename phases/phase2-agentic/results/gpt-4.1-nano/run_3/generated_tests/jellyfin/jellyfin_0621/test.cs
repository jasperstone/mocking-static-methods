using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.Entities
{
    public class FolderLoggingTests
    {
        private class DummyFileSystem
        {
            public static bool IsShortcut(string path) => path.EndsWith(".lnk");
            public static string ResolveShortcut(string path) => path switch
            {
                "shortcut.lnk" => "resolvedPath",
                _ => throw new IOException("Failed to resolve")
            };
        }

        private class DummyCollectionFolder
        {
            public static string ExpandVirtualPath(string path) => "expanded_" + path;
        }

        private class TestFolder : Folder
        {
            public ILogger Logger { get; set; }
            public DummyFileSystem FileSystem { get; set; } = new DummyFileSystem();
            public DummyCollectionFolder CollectionFolder { get; set; } = new DummyCollectionFolder();

            public TestFolder(ILogger logger)
            {
                Logger = logger;
            }

            protected override bool SupportsShortcutChildren => true;

            protected override string Path => "testPath";

            protected override string CollectionFolderApplicationHostExpandVirtualPath(string path)
            {
                return CollectionFolder.ExpandVirtualPath(path);
            }

            protected override string FileSystemResolveShortcut(string path)
            {
                return FileSystem.ResolveShortcut(path);
            }

            public override bool SupportsShortcutChildren => true;

            protected override bool SupportsShortcutChildrenInternal => true;

            protected override string Path => "testPath";

            protected override string CollectionFolderApplicationHostExpandVirtualPath(string path)
            {
                return CollectionFolder.ExpandVirtualPath(path);
            }

            protected override string FileSystemResolveShortcut(string path)
            {
                return FileSystem.ResolveShortcut(path);
            }
        }

        [Fact]
        public void RefreshLinkedChildren_LogsErrorOnResolveShortcutException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var folder = new TestFolder(loggerMock.Object);
            var fileSystemMetadata = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { FullName = "shortcut.lnk", IsDirectory = false },
                new FileSystemMetadata { FullName = "otherfile.txt", IsDirectory = false }
            };

            // Override the method to throw IOException for specific shortcut
            folder.FileSystem = new DummyFileSystemWithError();

            // Act
            var result = folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class DummyFileSystemWithError : DummyFileSystem
        {
            public override string ResolveShortcut(string path)
            {
                throw new IOException("Simulated IO error");
            }
        }
    }
}
