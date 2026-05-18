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
            var loggerMock = new Mock<ILogger>();
            var fileSystemMock = new Mock<IFileSystem>();
            var applicationHostMock = new Mock<IApplicationHost>();

            var folder = new TestFolder
            {
                Logger = loggerMock.Object,
                FileSystem = fileSystemMock.Object,
                ApplicationHost = applicationHostMock.Object
            };

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = "shortcut1.lnk",
                    IsDirectory = false
                }
            };

            fileSystemMock.Setup(fs => fs.IsShortcut(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(fs => fs.ResolveShortcut(It.IsAny<string>())).Returns("resolvedPath");
            applicationHostMock.Setup(ah => ah.ExpandVirtualPath(It.IsAny<string>())).Returns(string.Empty);

            // Act
            folder.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            loggerMock.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving shortcut shortcut1.lnk")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Mock interfaces for testing
    public interface IFileSystem
    {
        bool IsShortcut(string path);
        string ResolveShortcut(string path);
    }

    public interface IApplicationHost
    {
        string ExpandVirtualPath(string path);
    }

    // Mock FileSystemMetadata class for testing
    public class FileSystemMetadata
    {
        public string FullName { get; set; }
        public bool IsDirectory { get; set; }
    }

    // Mock Folder class for testing
    public class Folder
    {
        public ILogger Logger { get; set; }
        public IFileSystem FileSystem { get; set; }
        public IApplicationHost ApplicationHost { get; set; }

        public bool SupportsShortcutChildren => true;

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

                            var resolvedPath = ApplicationHost.ExpandVirtualPath(FileSystem.ResolveShortcut(i.FullName));

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

    // Derived class to expose protected method
    public class TestFolder : Folder
    {
        public new bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
        {
            return base.RefreshLinkedChildren(fileSystemChildren);
        }
    }

    public class LinkedChild
    {
        public string Path { get; set; }
        public LinkedChildType Type { get; set; }
    }

    public enum LinkedChildType
    {
        Shortcut,
        Manual
    }
}
