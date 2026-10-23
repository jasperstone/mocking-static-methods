using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Database.Providers.Sqlite;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Common.Configuration;
using Jellyfin.Database.Implementations.DbConfiguration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Jellyfin.Database.Implementations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Jellyfin.Database.Tests.Providers.Sqlite
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_LogsConnectionString()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockApplicationPaths = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = "SQLite"
            };
            var optionsBuilder = new DbContextOptionsBuilder();

            var provider = new SqliteDatabaseProvider(mockApplicationPaths.Object, mockLogger.Object);

            // Act
            provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Initialise_LogsSensitiveDataLoggingEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockApplicationPaths = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = "SQLite",
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
                    }
                }
            };
            var optionsBuilder = new DbContextOptionsBuilder();

            var provider = new SqliteDatabaseProvider(mockApplicationPaths.Object, mockLogger.Object);

            // Act
            provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimizationSuccess()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockApplicationPaths = new Mock<IApplicationPaths>();
            var mockDbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var mockDbContext = new Mock<JellyfinDbContext>();
            var mockDatabase = new Mock<DatabaseFacade>();

            mockDbContext.Setup(x => x.Database).Returns(mockDatabase.Object);
            mockDbContextFactory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(mockDbContext.Object);

            var provider = new SqliteDatabaseProvider(mockApplicationPaths.Object, mockLogger.Object)
            {
                DbContextFactory = mockDbContextFactory.Object
            };

            // Act
            await provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("jellyfin.db optimized successfully!")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
