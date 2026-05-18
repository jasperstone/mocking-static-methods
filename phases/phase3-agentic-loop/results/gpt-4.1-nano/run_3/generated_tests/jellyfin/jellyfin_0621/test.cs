using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Tests.Entities
{
    public class FolderTests
    {
        private readonly Mock<ILogger<Folder>> _loggerMock;
        private readonly Mock<LibraryManager> _libraryManagerMock;
        private readonly Folder _folder;

        public FolderTests()
        {
            _loggerMock = new Mock<ILogger<Folder>>();
            _libraryManagerMock = new Mock<LibraryManager>();
            _folder = new Folder
            {
                Logger = _loggerMock.Object,
                LibraryManager = _libraryManagerMock.Object,
                Path = "somepath",
                Id = Guid.NewGuid()
            };
        }

        [Fact]
        public void RefreshLinkedChildren_ShouldLogError_WhenShortcutResolutionFails()
        {
            // Arrange
            var fileSystemMetadata = new List<FileSystemMetadata>
            {
                new FileSystemMetadata
                {
                    FullName = "shortcut.lnk",
                    IsDirectory = false
                }
            };

            // Setup FileSystem.IsShortcut to return true for the test file
            // Since FileSystem is static, we need to assume it is mockable or replace with a wrapper
            // For this test, we will assume the method returns true, and simulate an exception in ResolveShortcut

            // We will simulate the ExpandVirtualPath to throw an IOException
            var folder = new Folder
            {
                Logger = _loggerMock.Object,
                LibraryManager = _libraryManagerMock.Object,
                SupportsShortcutChildren = true
            };

            // Act
            folder.RefreshLinkedChildren(fileSystemMetadata);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error resolving shortcut {0}", "shortcut.lnk"),
                Times.Once);
        }
    }
}
