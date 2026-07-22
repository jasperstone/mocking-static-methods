using System;
using System.Collections.Concurrent;
using System.IO;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StartWatchingPath_ThrowsException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new Mock<LibraryMonitor>(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreIgnoreRuleMock.Object);

            var path = "C:\\TestPath";

            // Create a subclass of LibraryMonitor to override StartWatchingPath
            var libraryMonitorSubclass = new Mock<LibraryMonitorSubclass>(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreIgnoreRuleMock.Object);

            // Act
            libraryMonitorSubclass.Object.StartWatchingPath(path);

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ),
                Times.Once);
        }

        private class LibraryMonitorSubclass : LibraryMonitor
        {
            public LibraryMonitorSubclass(
                ILogger<LibraryMonitor> logger,
                ILibraryManager libraryManager,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IHostApplicationLifetime appLifetime,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(logger, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule)
            {
            }

            public override void StartWatchingPath(string path)
            {
                throw new Exception("Test exception");
            }
        }
    }
}
