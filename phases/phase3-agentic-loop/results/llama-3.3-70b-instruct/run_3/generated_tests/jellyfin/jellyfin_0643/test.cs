using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace MediaBrowser.Controller.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteFile_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws<IOException>();
            var loggerMock = new Mock<ILogger>();
            var path = "test.txt";

            // Act
            FileSystemHelper.DeleteFile(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", path), Times.Once);
        }

        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(It.IsAny<string>())).Returns(new[] { "test" });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(It.IsAny<string>())).Returns(Enumerable.Empty<string>());
            fileSystemMock.Setup(fs => fs.Delete(It.IsAny<string>())).Throws<IOException>();
            var loggerMock = new Mock<ILogger>();
            var path = "test";

            // Act
            FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, path, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", path), Times.Once);
        }
    }
}
