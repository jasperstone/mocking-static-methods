using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.Database.Providers.Sqlite;
using Microsoft.EntityFrameworkCore;
using MediaBrowser.Common.Configuration;
using Jellyfin.Database.Implementations.DbConfiguration;
using Microsoft.Data.Sqlite;
using Jellyfin.Database.Implementations;

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
                DatabaseType = "SQLite"
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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
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
                DatabaseType = "SQLite",
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
    }
}
