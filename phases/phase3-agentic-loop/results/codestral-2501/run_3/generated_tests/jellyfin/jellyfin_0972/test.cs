using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Database.Providers.Sqlite;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Jellyfin.Database.Implementations.DbConfiguration;

namespace Jellyfin.Database.Tests.Providers.Sqlite
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption { Key = "path", Value = "test.db" }
                    }
                }
            };

            var optionsBuilder = new DbContextOptionsBuilder<JellyfinDbContext>();
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);

            // Act
            provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string: Data Source=test.db")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Initialise_LogsSensitiveDataLoggingEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
                    }
                }
            };

            var optionsBuilder = new DbContextOptionsBuilder<JellyfinDbContext>();
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);

            // Act
            provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimisationSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var dbContextMock = new Mock<JellyfinDbContext>();
            var databaseMock = new Mock<DatabaseFacade>();

            dbContextMock.Setup(x => x.Database).Returns(databaseMock.Object);
            dbContextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);

            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object)
            {
                DbContextFactory = dbContextFactoryMock.Object
            };

            // Act
            await provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            loggerMock.Verify(
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
