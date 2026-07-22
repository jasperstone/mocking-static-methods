using System;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public async Task ReportFileSystemChangeComplete_DoesNotLogError_WhenNoException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new DotIgnoreIgnoreRuleStub();

            // Setup appLifetime to allow Start and Stop registration without side effects
            appLifetimeMock.SetupGet(a => a.ApplicationStarted).Returns(new CancellationToken(false));
            appLifetimeMock.SetupGet(a => a.ApplicationStopping).Returns(new CancellationToken(false));
            appLifetimeMock.SetupGet(a => a.ApplicationStopped).Returns(new CancellationToken(false));
            appLifetimeMock.Setup(a => a.StopApplication());

            // Create the LibraryMonitor instance
            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock);

            var testPath = "C:\\TestPath";

            // Act
            monitor.ReportFileSystemChangeComplete(testPath, false);
            monitor.ReportFileSystemChangeComplete(testPath, true);

            // Wait a short time to allow async void to run
            await Task.Delay(100);

            // Assert
            // We expect no error logs because ReportFileSystemChanged does not throw in this test.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }

        // Minimal stub for DotIgnoreIgnoreRule
        private class DotIgnoreIgnoreRuleStub : DotIgnoreIgnoreRule
        {
        }
    }
}
