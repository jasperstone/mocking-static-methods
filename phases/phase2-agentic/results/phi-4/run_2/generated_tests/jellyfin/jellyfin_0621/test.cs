using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Entities.Tests
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutCannotBeResolved()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Folder>>();
            var fileSystemMetadata = new FileSystemMetadata
            {
                FullName = "testShortcut.lnk",
                IsDirectory = false
            };

            var folder = new Folder
            {
                Logger = loggerMock.Object,
                SupportsShortcutChildren = true
            };

            // Act
            folder.RefreshLinkedChildren(new[] { fileSystemMetadata });

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", fileSystemMetadata.FullName),
                Times.Once);

            loggerMock.Verify(
                x => x.LogError("Error resolving shortcut {0}", fileSystemMetadata.FullName),
                Times.Once);
        }
    }

    // Mock classes to support the test
    public class FileSystemMetadata
    {
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class Folder : BaseItem
    {
        public ILogger<Folder> Logger { get; set; }
        public bool SupportsShortcutChildren { get; set; }

        protected virtual bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
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

                            var resolvedPath = CollectionFolder.ApplicationHost.ExpandVirtualPath(FileSystem.ResolveShortcut(i.FullName));

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

                return false;
            }

            return false;
        }
    }

    public class BaseItem
    {
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

    public static class FileSystem
    {
        public static bool IsShortcut(string path) => true;
        public static string ResolveShortcut(string path) => null;
    }

    public static class CollectionFolder
    {
        public static class ApplicationHost
        {
            public static string ExpandVirtualPath(string path) => path;
        }
    }
}
