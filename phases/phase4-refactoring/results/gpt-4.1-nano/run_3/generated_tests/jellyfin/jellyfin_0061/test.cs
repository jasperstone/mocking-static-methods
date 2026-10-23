using System;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LibraryMonitorTests
{
    public class LibraryMonitorUnitTests
    {
        [Fact]
        public async Task ReportFileSystemChangeComplete_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();

            var monitor = new TestableLibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            var testPath = "C:\\TestPath";

            // Override ReportFileSystemChanged to throw
            monitor.ReportFileSystemChangedAction = () => throw new InvalidOperationException("Test exception");

            // Act
            await monitor.ReportFileSystemChangeComplete(testPath, true);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // Helper class to override the method
        private class TestableLibraryMonitor : LibraryMonitor
        {
            public Action ReportFileSystemChangedAction { get; set; }

            public TestableLibraryMonitor(
                ILogger<LibraryMonitor> logger,
                ILibraryManager libraryManager,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IHostApplicationLifetime appLifetime,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(logger, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule)
            {
            }

            protected override void ReportFileSystemChanged(string path)
            {
                if (ReportFileSystemChangedAction != null)
                {
                    ReportFileSystemChangedAction();
                }
                else
                {
                    base.ReportFileSystemChanged(path);
                }
            }
        }
    }
}
