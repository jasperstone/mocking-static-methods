using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Providers.Sqlite;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_LogsConnectionStringAndSensitiveDataLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockAppPaths = new Mock<IApplicationPaths>();
            mockAppPaths.SetupGet(x => x.DataPath).Returns("dataPath");

            var provider = new SqliteDatabaseProvider(mockAppPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new Collection<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
            };

            var dbConfigOptions = new DatabaseConfigurationOptions
            {
                DatabaseType = "SQLite",
                CustomProviderOptions = new CustomDatabaseOptions
                {
                    PluginName = "plugin",
                    PluginAssembly = "assembly",
                    ConnectionString = "connstring",
                    Options = customOptions
                }
            };

            // Act
            provider.Initialise(optionsBuilder, dbConfigOptions);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
