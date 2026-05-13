using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Providers.Sqlite;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_LogsConnectionString_And_LogsEnableSensitiveDataLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockAppPaths = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.DataPath).Returns("dataPath");

            var provider = new SqliteDatabaseProvider(mockAppPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
            };
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions { Options = customOptions }
            };

            // Act
            provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_ExecutesSqlCommands_And_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockAppPaths = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
            var provider = new SqliteDatabaseProvider(mockAppPaths.Object, mockLogger.Object);

            var mockDbContext = new Mock<JellyfinDbContext>(new DbContextOptions<JellyfinDbContext>());
            var mockDatabaseFacade = new Mock<DatabaseFacade>(mockDbContext.Object);

            mockDbContext.SetupGet(c => c.Database).Returns(mockDatabaseFacade.Object);

            mockDatabaseFacade.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var mockFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDbContext.Object);

            provider.DbContextFactory = mockFactory.Object;

            // Act
            await provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            mockDatabaseFacade.Verify(d => d.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE)", It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockDatabaseFacade.Verify(d => d.ExecuteSqlRawAsync("PRAGMA optimize", It.IsAny<CancellationToken>()), Times.Once);
            mockDatabaseFacade.Verify(d => d.ExecuteSqlRawAsync("VACUUM", It.IsAny<CancellationToken>()), Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("jellyfin.db optimized successfully!")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Minimal stubs for types used in the provider
    public class CustomDatabaseOption
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }

    public class CustomProviderOptions
    {
        public ICollection<CustomDatabaseOption>? Options { get; set; }
    }

    public class DatabaseConfigurationOptions
    {
        public CustomProviderOptions? CustomProviderOptions { get; set; }
    }

    public class JellyfinDbContext : DbContext
    {
        public JellyfinDbContext(DbContextOptions options) : base(options) { }
    }
}
