using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Jellyfin.Server.Implementations.SystemBackupService;
using MediaBrowser.Controller;
using MediaBrowser.Controller.SystemBackupService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsWarningWhenStarting()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            var service = new BackupService(
                loggerMock.Object,
                dbFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                databaseProviderMock.Object,
                hostLifetimeMock.Object);

            var archivePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.zip");

            // Act
            await Assert.ThrowsAsync<FileNotFoundException>(() => service.RestoreBackupAsync(archivePath));

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => CheckLogState(state, "Begin restoring system to {BackupArchive}", archivePath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool CheckLogState(object state, string expectedTemplate, string expectedArchivePath)
        {
            if (state is not IEnumerable<KeyValuePair<string, object?>> stateProperties)
            {
                return false;
            }

            var properties = stateProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return properties.TryGetValue("{OriginalFormat}", out var template)
                && string.Equals(template?.ToString(), expectedTemplate, StringComparison.Ordinal)
                && properties.TryGetValue("BackupArchive", out var archivePath)
                && string.Equals(archivePath?.ToString(), expectedArchivePath, StringComparison.Ordinal)
                && string.Equals(state.ToString(), $"Begin restoring system to {expectedArchivePath}", StringComparison.Ordinal);
        }
    }
}
