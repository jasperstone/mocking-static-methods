using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MediaBrowser.Controller.Entities.Tests")]

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        public class FileSystemMetadata
        {
            public string FullName { get; set; }
            public bool IsDirectory { get; set; }
        }

        public interface IFileSystem
        {
            bool IsShortcut(string path);
            string ResolveShortcut(string path);
        }

        public class Folder
        {
            public ILogger Logger { get; set; }
            public IFileSystem FileSystem { get; set; }
            public ICollectionFolder CollectionFolder { get; set; }
            public bool SupportsShortcutChildren => true;

            internal virtual bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
            {
                if (SupportsShortcutChildren)
                {
                    var newShortcutLinks = fileSystemChildren
                        .Where(i => !i.IsDirectory && FileSystem.IsShortcut(i.FullName))
                        .Select(i =>
                        {
                            try
                            {
                                Logger.LogDebug("Found shortcut at {0}", i.FullName);

                                var resolvedPath = CollectionFolder.ExpandVirtualPath(FileSystem.ResolveShortcut(i.FullName));

                                if (!string.IsNullOrEmpty(resolvedPath))
                                {
                                    return new LinkedChild
                                    {
                                        Path = resolvedPath,
                                        Type = LinkedChildType.Shortcut
                                    };
                                }

                                Logger.LogError("Error resolving shortcut {0}", i.FullName);

                                return null;
                            }
                            catch (IOException ex)
                            {
                                Logger.LogError(ex, "Error resolving shortcut {0}", i.FullName);
                                return null;
                            }
                        })
                        .Where(i => i is not null)
                        .ToList();

                    return newShortcutLinks.Count > 0;
                }

                return false;
            }
        }

        public class LinkedChild
        {
            public string Path { get; set; }
            public LinkedChildType Type { get; set; }
        }

        public enum LinkedChildType
        {
            Shortcut
        }

        public interface ICollectionFolder
        {
            string ExpandVirtualPath(string path);
        }

        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutCannotBeResolved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var collectionFolderMock = new Mock<ICollectionFolder>();

            var folder = new Folder
            {
                Logger = loggerMock.Object,
                FileSystem = fileSystemMock.Object,
                CollectionFolder = collectionFolderMock.Object
            };

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = "path/to/shortcut.lnk",
                    IsDirectory = false
                }
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(It.IsAny<string>())).Returns((string)null);
            collectionFolderMock.Setup(cf => cf.ExpandVirtualPath(It.IsAny<string>())).Returns((string)null);

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut path/to/shortcut.lnk")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
