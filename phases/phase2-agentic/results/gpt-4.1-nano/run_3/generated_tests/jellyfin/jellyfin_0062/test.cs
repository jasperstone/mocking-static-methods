using System;
using System.Collections.Generic;
using System.IO;
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
            var started = new Mock<Action>();
            var stopping = new Mock<Action>();
            appLifetime.Setup(a => a.ApplicationStarted).Returns(new CancellationToken());
            appLifetime.Setup(a => a.ApplicationStopping).Returns(new CancellationToken());

            _libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreMock.Object);
        }

        [Fact]
        public async Task ReportFileSystemChangeComplete_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var path = "testPath";
            var exception = new Exception("Test exception");
            var logCalled = false;

            _loggerMock.Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()))
                .Callback<Exception, string, object>((ex, msg, arg) =>
                {
                    logCalled = true;
                    Assert.Equal(exception, ex);
                    Assert.Contains(path, msg);
                });

            // Act
            await _libraryMonitor.ReportFileSystemChangeComplete(path, true);

            // Assert
            Assert.True(logCalled);
        }

        [Fact]
        public void ContainsParentFolder_Should_Return_True_For_ParentPath()
        {
            // Arrange
            var list = new List<string> { @"C:\Music", @"D:\Videos" };
            var path = @"C:\Music\Album";

            // Act
            var result = LibraryMonitor.ContainsParentFolder(list, path.AsSpan());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ContainsParentFolder_Should_Return_False_For_NonParentPath()
        {
            // Arrange
            var list = new List<string> { @"C:\Music", @"D:\Videos" };
            var path = @"E:\Photos";

            // Act
            var result = LibraryMonitor.ContainsParentFolder(list, path.AsSpan());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ContainsParentFolder_Should_Throw_For_Empty_Path()
        {
            // Arrange
            var list = new List<string> { @"C:\Music" };
            var path = ReadOnlySpan<char>.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => LibraryMonitor.ContainsParentFolder(list, path));
        }
    }
}
