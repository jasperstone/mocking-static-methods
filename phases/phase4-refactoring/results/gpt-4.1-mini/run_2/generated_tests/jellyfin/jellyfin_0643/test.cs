using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.IO.Tests
{
    public class FileSystemHelperTests
    {
        private static void VerifyLogErrorCalled(Mock<ILogger> mockLogger, Exception ex, string path)
        {
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(path)),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void DeleteFile_LogsErrorOnIOException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();

            var ioException = new IOException("Test IO exception");
            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(ioException);

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, "somepath", mockLogger.Object);

            // Assert
            VerifyLogErrorCalled(mockLogger, ioException, "somepath");
        }

        [Fact]
        public void DeleteFile_LogsErrorOnUnauthorizedAccessException()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystem>();
            var mockLogger = new Mock<ILogger>();

            var unauthorizedException = new UnauthorizedAccessException("Test unauthorized access");
            mockFileSystem.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(unauthorizedException);

            // Act
            FileSystemHelper.DeleteFile(mockFileSystem.Object, "somepath", mockLogger.Object);

            // Assert
            VerifyLogErrorCalled(mockLogger, unauthorizedException, "somepath");
        }
    }
}
