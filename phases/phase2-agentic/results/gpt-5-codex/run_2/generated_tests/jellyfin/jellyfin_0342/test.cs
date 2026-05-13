using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Server.Implementations.FullSystemBackup;
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
        public async Task RestoreBackupAsync_LogsWarningWhenBeginningRestore()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var applicationHostMock = new Mock<IServerApplicationHost>();
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var hostApplicationLifetimeMock = new Mock<IHostApplicationLifetime>();

            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                applicationHostMock.Object,
                applicationPathsMock.Object,
                jellyfinDatabaseProviderMock.Object,
                hostApplicationLifetimeMock.Object);

            var archivePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            // Act
            await Assert.ThrowsAsync<FileNotFoundException>(() => backupService.RestoreBackupAsync(archivePath));

            // Assert
            var logInvocation = Assert.Single(loggerMock.Invocations);
            Assert.Equal("Log", logInvocation.Method.Name);
            Assert.Equal(LogLevel.Warning, (LogLevel)logInvocation.Arguments[0]);
            Assert.Null(logInvocation.Arguments[3]);

            var state = logInvocation.Arguments[2];
            var stateList = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(state);

            Assert.Equal($"Begin restoring system to {archivePath}", state.ToString());

            var originalFormat = stateList.First(kvp => kvp.Key == "{OriginalFormat}").Value;
            Assert.Equal("Begin restoring system to {BackupArchive}", originalFormat);

            var archiveArgument = stateList.First(kvp => kvp.Key == "BackupArchive").Value;
            Assert.Equal(archivePath, archiveArgument);
        }
    }
}
