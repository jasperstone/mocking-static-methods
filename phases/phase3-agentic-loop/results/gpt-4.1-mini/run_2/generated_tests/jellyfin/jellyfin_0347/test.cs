using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.FullSystemBackup.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsDatabasePurgedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<DbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<DbContext>>();
            var jellyfinDatabaseProviderMock = new Mock<object>();
            var hostApplicationLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();

            // Setup dbContext to have a Model with entity types
            var modelMock = new Mock<Microsoft.EntityFrameworkCore.Metadata.IModel>();
            dbContextMock.SetupGet(d => d.Model).Returns(modelMock.Object);
            dbContextMock.SetupGet(d => d.ChangeTracker).Returns(Mock.Of<ChangeTracker>());
            dbContextMock.SetupSet(d => d.ChangeTracker.QueryTrackingBehavior = It.IsAny<QueryTrackingBehavior>());

            // Setup dbContextFactory to return our dbContext mock
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            // Setup jellyfinDatabaseProvider to simulate PurgeDatabase call
            // We cannot mock IJellyfinDatabaseProvider because type is unknown, so use dynamic invocation
            var purgeCalled = false;
            var jellyfinDatabaseProvider = new Mock<IJellyfinDatabaseProviderMock>();
            jellyfinDatabaseProvider.Setup(j => j.PurgeDatabase(It.IsAny<DbContext>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Task.CompletedTask)
                .Callback(() => purgeCalled = true);

            // Create a temporary zip archive with minimal required entries
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fs = new FileStream(tempFile, FileMode.Create))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, true))
                {
                    // Add manifest.json entry with minimal valid content
                    var manifestEntry = archive.CreateEntry("manifest.json");
                    using (var entryStream = manifestEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("{\"ServerVersion\":\"1.0.0\",\"BackupEngineVersion\":\"0.2.0\",\"Options\":{\"Database\":true}}");
                    }

                    // Add HistoryRow.json entry to avoid exception
                    var historyEntry = archive.CreateEntry("Database/HistoryRow.json");
                    using (var entryStream = historyEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.Write("[]");
                    }
                }

                var backupService = new BackupService(
                    loggerMock.Object,
                    dbContextFactoryMock.Object,
                    null!, // applicationHost - not used in this test path
                    null!, // applicationPaths - not used in this test path
                    jellyfinDatabaseProvider.Object,
                    hostApplicationLifetimeMock.Object);

                // Act
                await backupService.RestoreBackupAsync(tempFile);

                // Assert
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Database Purged")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce);

                Assert.True(purgeCalled, "PurgeDatabase should have been called.");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        // Dummy interface to mock IJellyfinDatabaseProvider methods
        public interface IJellyfinDatabaseProviderMock
        {
            Task PurgeDatabase(DbContext dbContext, IEnumerable<string> tableNames);
        }
    }
}
