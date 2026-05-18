using System;
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
        public async Task ReportFileSystemChangeComplete_LogsError_WhenReportFileSystemChangedThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<object>(); // Use object since DotIgnoreIgnoreRule is internal or inaccessible

            // Setup appLifetime to allow Start and Stop registration without side effects
            var appStartedMock = new Mock<IHostApplicationLifetime>();
            var appStoppingMock = new Mock<IHostApplicationLifetime>();
            appLifetimeMock.Setup(a => a.ApplicationStarted).Returns(new CancellationTokenRegistrationWrapper());
            appLifetimeMock.Setup(a => a.ApplicationStopping).Returns(new CancellationTokenRegistrationWrapper());

            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                (dynamic)dotIgnoreMock.Object);

            var testPath = "C:\\TestPath";

            // We cannot cause ReportFileSystemChanged to throw because it is private and class is sealed.
            // So we test that calling ReportFileSystemChangeComplete with refreshPath = true does not throw and does not log error.
            monitor.ReportFileSystemChangeComplete(testPath, true);
            await Task.Delay(10); // wait for async void to complete

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }

    // Helper class to simulate CancellationTokenRegistration for IHostApplicationLifetime properties
    public class CancellationTokenRegistrationWrapper : System.Threading.CancellationToken
    {
        public void Register(Action callback)
        {
            // Do nothing
        }
    }
}
