using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.IO;

namespace Emby.Tests.IO
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly DotIgnoreIgnoreRule _dotIgnoreIgnoreRule;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var appLifetime = _appLifetimeMock.Object;
            appLifetime.ApplicationStarted = new Mock<IApplicationLifetime>().Object;
            appLifetime.ApplicationStopping = new Mock<IApplicationLifetime>().Object;
        }

        [Fact]
        public void LogError_IsCalled_When_ExceptionOccursInReportFileSystemChangeComplete()
        {
            // Arrange
            var monitor = new TestLibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreIgnoreRule);

            var testPath = "testPath";

            // Setup the logger to verify LogError call
            _loggerMock.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true))
            ).Verifiable();

            // Act
            // Use reflection to invoke the private method that triggers exception
            var method = typeof(LibraryMonitor).GetMethod("ReportFileSystemChangeComplete", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(monitor, new object[] { testPath, true });

            // Assert
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        // A subclass to override the method for testing
        private class TestLibraryMonitor : LibraryMonitor
        {
            public TestLibraryMonitor(
                ILogger<LibraryMonitor> logger,
                ILibraryManager libraryManager,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IHostApplicationLifetime appLifetime,
                DotIgnoreIgnoreRule dotIgnoreIgnoreRule)
                : base(logger, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule)
            {
            }
        }
    }
}
