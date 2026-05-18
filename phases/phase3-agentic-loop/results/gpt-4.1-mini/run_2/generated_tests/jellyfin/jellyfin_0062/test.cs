using System;
using System.IO;
using System.Reflection;
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
        private class DummyDotIgnoreIgnoreRule { }

        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnore = new DummyDotIgnoreIgnoreRule();

            // Setup appLifetime to allow registration without error
            var startedTokenSource = new System.Threading.CancellationTokenSource();
            var stoppingTokenSource = new System.Threading.CancellationTokenSource();

            appLifetimeMock.SetupGet(a => a.ApplicationStarted).Returns(startedTokenSource.Token);
            appLifetimeMock.SetupGet(a => a.ApplicationStopping).Returns(stoppingTokenSource.Token);
            appLifetimeMock.SetupGet(a => a.ApplicationStopped).Returns(new System.Threading.CancellationToken(false));
            appLifetimeMock.Setup(a => a.StopApplication()).Verifiable();

            var libraryMonitor = (LibraryMonitor)Activator.CreateInstance(
                typeof(LibraryMonitor),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new object[]
                {
                    loggerMock.Object,
                    libraryManagerMock.Object,
                    configManagerMock.Object,
                    fileSystemMock.Object,
                    appLifetimeMock.Object,
                    dotIgnore
                },
                null)!;

            // Create a FileSystemWatcher with a fake path
            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Act
            // Use reflection to call the private DisposeWatcher method
            var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(libraryMonitor, new object[] { watcher, false });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\TestPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
