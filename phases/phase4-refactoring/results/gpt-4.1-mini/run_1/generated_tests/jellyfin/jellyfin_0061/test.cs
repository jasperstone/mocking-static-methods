using System;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
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

            // Setup CancellationToken properties for ApplicationStarted and ApplicationStopping
            appLifetimeMock.SetupGet(a => a.ApplicationStarted).Returns(CancellationToken.None);
            appLifetimeMock.SetupGet(a => a.ApplicationStopping).Returns(CancellationToken.None);

            var dotIgnore = new DotIgnoreIgnoreRule();

            // Setup libraryManager to throw when ReportFileSystemChanged indirectly calls GetLibraryOptions
            libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<BaseItem>())).Throws(new InvalidOperationException("Test exception"));

            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnore);

            var testPath = "C:\\TestPath";

            // Act
            // Call the async void method and wait a bit to let it run
            monitor.ReportFileSystemChangeComplete(testPath, true);

            // Wait enough time for the async void to complete (less than 45s delay for test)
            await Task.Delay(100);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
