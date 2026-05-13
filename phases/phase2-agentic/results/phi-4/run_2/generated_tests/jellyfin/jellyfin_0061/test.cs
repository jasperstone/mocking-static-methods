using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void ReportFileSystemChangeComplete_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                null, // Mock IHostApplicationLifetime
                dotIgnoreIgnoreRuleMock.Object);

            // Simulate an exception in ReportFileSystemChanged
            libraryManagerMock.Setup(x => x.ReportFileSystemChanged(It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            libraryMonitor.ReportFileSystemChangeComplete("testPath", true);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error in ReportFileSystemChanged for {Path}",
                    "testPath"),
                Times.Once);
        }
    }
}
