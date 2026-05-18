using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Providers.Sqlite;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Database.Tests.Providers.Sqlite
{
    public class SqliteDatabaseProviderTests
    {
        private readonly Mock<IApplicationPaths> _mockApplicationPaths;
        private readonly Mock<ILogger<SqliteDatabaseProvider>> _mockLogger;
        private readonly SqliteDatabaseProvider _provider;

        public SqliteDatabaseProviderTests()
        {
            _mockApplicationPaths = new Mock<IApplicationPaths>();
            _mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
        }

        [Fact]
        public void Initialise_LogsConnectionString_And_LogsEnableSensitiveDataLogging_WhenEnabled()
        {
            // Arrange
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            _mockApplicationPaths.Setup(ap => ap.DataPath).Returns(dataPath);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = bool.TrueString }
            };

            var databaseConfig = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomDatabaseOptions { Options = customOptions }
            };

            // Act
            _provider.Initialise(optionsBuilder, databaseConfig);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Initialise_LogsConnectionString_And_DoesNotLogEnableSensitiveDataLogging_WhenDisabled()
        {
            // Arrange
            var dataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dataPath);
            _mockApplicationPaths.Setup(ap => ap.DataPath).Returns(dataPath);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = bool.FalseString }
            };

            var databaseConfig = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomDatabaseOptions { Options = customOptions }
            };

            // Act
            _provider.Initialise(optionsBuilder, databaseConfig);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
