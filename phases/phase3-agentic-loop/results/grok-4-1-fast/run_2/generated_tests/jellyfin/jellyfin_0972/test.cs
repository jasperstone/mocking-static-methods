using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        private readonly Mock<IApplicationPaths> _mockApplicationPaths;
        private readonly Mock<ILogger<SqliteDatabaseProvider>> _mockLogger;
        private readonly SqliteDatabaseProvider _provider;

        public SqliteDatabaseProviderTests()
        {
            _mockApplicationPaths = new Mock<IApplicationPaths>();
            _mockApplicationPaths.Setup(x => x.DataPath).Returns("/test/path");
            _mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void Initialise_EnablesSensitiveDataLogging_LogsInformationMessage()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = "Jellyfin-SQLite",
                CustomProviderOptions = new CustomDatabaseOptions
                {
                    PluginName = "test",
                    PluginAssembly = "test",
                    ConnectionString = "test",
                    Options = new Collection<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
                    }
                }
            };

            // Act
            _provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null!, null!)!.Contains("EnableSensitiveDataLogging is enabled on SQLite connection"))),
                Times.Once);
        }

        [Fact]
        public void Initialise_DoesNotEnableSensitiveDataLogging_DoesNotLogInformationMessage()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = "Jellyfin-SQLite"
            };

            // Act
            _provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null!, null!)!.Contains("EnableSensitiveDataLogging is enabled on SQLite connection"))),
                Times.Never);
        }

        [Fact]
        public void Initialise_AlwaysLogsConnectionStringInformation()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = "Jellyfin-SQLite"
            };

            // Act
            _provider.Initialise(optionsBuilder, databaseConfiguration);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>(formatter => 
                        formatter(null!, null!)!.Contains("SQLite connection string:"))),
                Times.Once);
        }
    }
}
