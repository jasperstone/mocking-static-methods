using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.Entities
{
    public class FolderTests
    {
        [Fact]
        public void RefreshLinkedChildren_LogsError_WhenShortcutResolutionFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<Folder>>();
            var mockFileSystem = new Mock<IFileSystem>();
            var folder = new Mock<FolderSubclass> { CallBase = true };
            folder.Setup(f => f.SupportsShortcutChildren).Returns(true);
            folder.Setup(f => f.Logger).Returns(mockLogger.Object);
            folder.Setup(f => f.FileSystem).Returns(mockFileSystem.Object);

            var fileSystemChildren = new List<FileSystemMetadata>
            {
                new FileSystemMetadata { IsDirectory = false, FullName = "shortcut1.lnk" }
            };

            mockFileSystem.Setup(fs => fs.IsShortcut("shortcut1.lnk")).Returns(true);
            mockFileSystem.Setup(fs => fs.ResolveShortcut("shortcut1.lnk")).Throws(new IOException());

            // Act
            folder.Object.RefreshLinkedChildren(fileSystemChildren);

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<IOException>(),
                    It.Is<string>(s => s.Contains("Error resolving shortcut")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }

    public class FolderSubclass : Folder
    {
        public new ILogger Logger { get; set; }
        public new IFileSystem FileSystem { get; set; }
        public new bool SupportsShortcutChildren { get; set; }
        public new bool RefreshLinkedChildren(IEnumerable<FileSystemMetadata> fileSystemChildren)
        {
            return base.RefreshLinkedChildren(fileSystemChildren);
        }
    }
}
