using Moq;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Emby.Server.Implementations; // Added using directive for ILibraryManager, IServerConfigurationManager, IFileSystem, DotIgnoreIgnoreRule
using MediaBrowser.Controller.Entities; // Assuming DotIgnoreIgnoreRule is in this namespace

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

            var path = "testPath";
            var exception = new InvalidOperationException("Test exception"); // Use a more specific exception type

            // Act
            libraryMonitor.ReportFileSystemChangeComplete(path, true);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<Exception>(e => e.Message == "Test exception"), // Ensure the exception message is consistent
                    It.Is<string>(s => s == "Error in ReportFileSystemChanged for {Path}"),
                    path),
                Times.Once);
        }
    }
}
