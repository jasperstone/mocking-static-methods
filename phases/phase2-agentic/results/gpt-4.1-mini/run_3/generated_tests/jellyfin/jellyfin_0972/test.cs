using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Providers.Sqlite;
using Jellyfin.Database.Implementations.DbConfiguration;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Jellyfin.Database.Tests.Providers.Sqlite
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

            var dbConfigOptions = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomDatabaseOptions { Options = customOptions }
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

        [Fact]
        public void Initialise_DoesNotLogEnableSensitiveDataLogging_WhenOptionIsFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockAppPaths = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();
            mockAppPaths.SetupGet(p => p.DataPath).Returns("dataPath");

            var provider = new SqliteDatabaseProvider(mockAppPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "false" }
            };

            var dbConfigOptions = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomDatabaseOptions { Options = customOptions }
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
                Times.Never);
        }
    }
}
