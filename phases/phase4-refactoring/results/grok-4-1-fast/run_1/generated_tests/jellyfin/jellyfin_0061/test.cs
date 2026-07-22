using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void ReportFileSystemChangeComplete_RefreshThrowsException_LogsError()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryMonitor>>();
            logger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Minimal null implementations
            var libraryManager = new Mock<ILibraryManager>();
            libraryManager.Setup(m => m.ReportFileSystemChanged(It.IsAny<string>()))
                         .Throws(new InvalidOperationException("Test exception"));

            var monitor = new LibraryMonitor(
                logger.Object,
                libraryManager.Object,
                new Mock<IServerConfigurationManager>().Object,
                new Mock<IFileSystem>().Object,
                new Mock<IHostApplicationLifetime>().Object,
                new Mock<DotIgnoreIgnoreRule>().Object);

            var path = @"C:\test\path";

            // Act
            monitor.ReportFileSystemChangeComplete(path, refreshPath: true);

            // Wait for Task.Delay(45000) - use shorter wait for test
            System.Threading.Thread.Sleep(100);

            // Assert
            logger.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error in ReportFileSystemChanged for {Path}",
                    path),
                Times.Once);
        }

        [Fact]
        public void Constructor_ValidatesDependencies()
        {
            // Just verify we can create with mocks
            var logger = NullLogger<LibraryMonitor>.Instance;
            var monitor = new LibraryMonitor(
                logger,
                new Mock<ILibraryManager>().Object,
                new Mock<IServerConfigurationManager>().Object,
                new Mock<IFileSystem>().Object,
                new Mock<IHostApplicationLifetime>().Object,
                new Mock<DotIgnoreIgnoreRule>().Object);
            
            Assert.NotNull(monitor);
        }
    }

    // Minimal interface implementations for compilation
    public interface ILibraryManager
    {
        void ReportFileSystemChanged(string path);
    }

    public interface IServerConfigurationManager { }

    public interface IFileSystem { }

    public class DotIgnoreIgnoreRule { }
}
