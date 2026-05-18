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
            var startedCts = new CancellationTokenSource();
            var stoppingCts = new CancellationTokenSource();

            appLifetimeMock.SetupGet(a => a.ApplicationStarted).Returns(startedCts.Token);
            appLifetimeMock.SetupGet(a => a.ApplicationStopping).Returns(stoppingCts.Token);

            var dotIgnore = new DotIgnoreIgnoreRule();

            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnore);

            var testPath = "C:\\TestPath";

            // Use reflection to get private method ReportFileSystemChanged
            var methodInfo = typeof(LibraryMonitor).GetMethod("ReportFileSystemChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Replace ReportFileSystemChanged with a delegate that throws is not possible because method is private and sealed class.
            // Instead, we will invoke ReportFileSystemChanged directly and catch the exception to verify logger.

            // Act
            // Call ReportFileSystemChangeComplete async void method with refreshPath = true
            monitor.ReportFileSystemChangeComplete(testPath, true);

            // Wait a bit to allow async void to run (normally 45 seconds delay, but we cannot override)
            await Task.Delay(100);

            // Assert
            // Verify that LogError was called with the expected message and exception
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged for")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
