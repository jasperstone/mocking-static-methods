using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Server.Implementations.FullSystemBackup;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Tests
{
    public class BackupServiceTests
    {
        [Fact]
        public async Task RestoreBackupAsync_LogsBeginPurgingDatabase()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<BackupService>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync()).ReturnsAsync(dbContextMock.Object);

            var applicationHostMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var applicationPathsMock = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>(); // placeholder, replace with actual type
            var jellyfinDatabaseProviderMock = new Mock<IJellyfinDatabaseProvider>();

            // Setup the factory to return the mock db context
            var backupService = new BackupService(
                loggerMock.Object,
                dbContextFactoryMock.Object,
                null, // applicationHost
                null, // applicationPaths
                jellyfinDatabaseProviderMock.Object,
                applicationHostMock.Object);

            // Create a dummy zip archive in memory
            var memStream = new MemoryStream();
            using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, true))
            {
                // Add manifest.json with server version
                var manifestEntry = archive.CreateEntry("manifest.json");
                using (var entryStream = manifestEntry.Open())
                {
                    var manifest = new
                    {
                        ServerVersion = "1.0.0",
                        BackupEngineVersion = "0.2.0"
                    };
                    JsonSerializer.Serialize(entryStream, manifest);
                }

                // Add a dummy database entry
                var dbEntry = archive.CreateEntry("Database/HistoryRow.json");
                using (var entryStream = dbEntry.Open())
                {
                    JsonSerializer.Serialize(entryStream, new { });
                }
            }
            memStream.Seek(0, SeekOrigin.Begin);

            // Save to a temp file
            var tempFilePath = Path.GetTempFileName();
            await using (var fileStream = File.Create(tempFilePath))
            {
                await memStream.CopyToAsync(fileStream);
            }

            // Act
            await backupService.RestoreBackupAsync(tempFilePath);

            // Assert
            // Verify that LogInformation("Begin restoring Database") was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Begin restoring Database")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
