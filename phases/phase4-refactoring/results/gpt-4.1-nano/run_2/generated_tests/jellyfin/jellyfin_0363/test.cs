using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Implementations.FullSystemBackup;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task CreateBackup_ShouldLogConfigurationFolder()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostLifetimeMock = new Mock<IHostApplicationLifetime>();

            var testConfigPath = "TestConfigPath";

            // Setup the mock for ConfigurationDirectoryPath
            applicationPathsMock.Setup(p => p.ConfigurationDirectoryPath).Returns(testConfigPath);

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                databaseProviderMock.Object,
                hostLifetimeMock.Object);

            // Act
            await backupService.CreateBackupAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(testConfigPath)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
