using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreRuleMock;

        private readonly LibraryMonitor _libraryMonitor;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var appLifetime = new Mock<IHostApplicationLifetime>();
            var started = new CancellationTokenSource();
            var stopping = new CancellationTokenSource();

            appLifetime.Setup(a => a.ApplicationStarted).Returns(new CancellationTokenRegistration(started.Token));
            appLifetime.Setup(a => a.ApplicationStopping).Returns(new CancellationTokenRegistration(stopping.Token));

            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                appLifetime.Object,
                _dotIgnoreRuleMock.Object);
        }

        [Fact]
        public async Task ReportFileSystemChangeComplete_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var testPath = "test/path";
            var exception = new Exception("Test exception");
            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()))
                .Verifiable();

            // Act
            await _libraryMonitor.ReportFileSystemChangeComplete(testPath, true);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error in ReportFileSystemChanged for {Path}", testPath), Times.Once);
        }

        [Fact]
        public void ContainsParentFolder_Should_Return_True_For_Parent_Path()
        {
            // Arrange
            var list = new List<string> { @"C:\Music" };
            var path = @"C:\Music\Album";

            // Act
            var result = LibraryMonitor.ContainsParentFolder(list, path.AsSpan());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsParentFolder_Should_Return_False_For_Non_Parent_Path()
        {
            // Arrange
            var list = new List<string> { @"C:\Music" };
            var path = @"D:\Videos";

            // Act
            var result = LibraryMonitor.ContainsParentFolder(list, path.AsSpan());

            // Assert
            Assert.False(result);
        }
    }
}
