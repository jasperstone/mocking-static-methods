using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public async Task StartWatchingPath_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var libraryMonitor = new LibraryMonitor(_loggerMock.Object, _libraryManagerMock.Object, _configurationManagerMock.Object, _fileSystemMock.Object, _appLifetimeMock.Object, _dotIgnoreIgnoreRuleMock.Object);
            var path = "path";
            _fileSystemMock.Setup(fs => fs.CreateDirectory(It.IsAny<string>())).Throws(new Exception("Test exception"));

            // Act
            libraryMonitor.StartWatchingPath(path);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", path), Times.Once);
        }
    }
}
