using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Linq;
using MediaBrowser.Model.IO;
using Xunit;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        [Fact]
        public void DeleteEmptyFolders_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var directoryPath = "/path/to/directory";
            fileSystemMock.Setup(fs => fs.GetDirectoryPaths(directoryPath)).Returns(new[] { directoryPath });
            fileSystemMock.Setup(fs => fs.GetFileSystemEntryPaths(directoryPath)).Returns(Enumerable.Empty<string>());
            fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(new IOException());

            // Act
            FileSystemHelper.DeleteEmptyFolders(fileSystemMock.Object, directoryPath, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting directory {Path}", directoryPath), Times.Once);
        }

        [Fact]
        public void DeleteFile_LogsError_WhenIOExceptionOccurs()
        {
            // Arrange
            var fileSystemMock = new Mock<IFileSystem>();
            var loggerMock = new Mock<ILogger>();
            var filePath = "/path/to/file";
            fileSystemMock.Setup(fs => fs.DeleteFile(filePath)).Throws(new IOException());

            // Act
            FileSystemHelper.DeleteFile(fileSystemMock.Object, filePath, loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<IOException>(), "Error deleting file {Path}", filePath), Times.Once);
        }
    }
}
