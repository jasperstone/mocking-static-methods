using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Tests.IO
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreMock;
        private readonly LibraryMonitor _libraryMonitor;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            var appLifetime = new Mock<IHostApplicationLifetime>();
            var started = new System.Threading.ManualResetEvent(false);
            var stopping = new System.Threading.ManualResetEvent(false);
            appLifetime.Setup(a => a.ApplicationStarted).Returns(new Microsoft.Extensions.Primitives.CancellationTokenSource().Token);
            appLifetime.Setup(a => a.ApplicationStopping).Returns(new Microsoft.Extensions.Primitives.CancellationTokenSource().Token);

            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);
        }

        [Fact]
        public void LogError_IsCalled_When_ReportFileSystemChangeComplete_Throws()
        {
            // Arrange
            var path = "somepath";
            var exception = new Exception("Test exception");
            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);

            // Use reflection to invoke the private method
            var method = typeof(LibraryMonitor).GetMethod("ReportFileSystemChangeComplete", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Act & Assert
            var task = (Task)method.Invoke(_libraryMonitor, new object[] { path, true });
            task.GetAwaiter().GetResult();

            // Verify that LogError was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error in ReportFileSystemChanged for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
