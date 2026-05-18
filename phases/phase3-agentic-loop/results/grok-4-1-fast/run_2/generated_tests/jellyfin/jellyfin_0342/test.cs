using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using System.Collections.Generic;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IDbContextFactory<object>> _dbProviderMock;
        private readonly Mock<object> _appHostMock;
        private readonly Mock<object> _appPathsMock;
        private readonly Mock<object> _jellyfinDbProviderMock;
        private readonly Mock<IHostApplicationLifetime> _lifetimeMock;
        private readonly BackupService _backupService;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _dbProviderMock = new Mock<IDbContextFactory<object>>();
            _appHostMock = new Mock<object>();
            _appPathsMock = new Mock<object>();
            _jellyfinDbProviderMock = new Mock<object>();
            _lifetimeMock = new Mock<IHostApplicationLifetime>();

            _backupService = new BackupService(
                _loggerMock.Object,
                _dbProviderMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object,
                _jellyfinDbProviderMock.Object,
                _lifetimeMock.Object);
        }

        [Fact]
        public async Task RestoreBackupAsync_LogsWarningAtStart()
        {
            // Arrange
            string testArchivePath = "test-backup.zip";
            
            // Setup logger to capture the log call
            _loggerMock.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<object, Exception?, string>>(
                    (level, eventId, state, ex, formatter) => {
                        var message = formatter(state, ex);
                        Assert.Equal("Begin restoring system to {BackupArchive}", message);
                    });

            // Act - let it throw FileNotFoundException, which happens after the log call
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _backupService.RestoreBackupAsync(testArchivePath));

            // Assert - verify the LogWarning extension was called (captured above)
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object state, Type _) => {
                        var message = state?.ToString();
                        return message == "Begin restoring system to {BackupArchive}";
                    }),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
