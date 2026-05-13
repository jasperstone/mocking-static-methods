using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        private readonly Mock<IApplicationPaths> _applicationPathsMock;
        private readonly Mock<ILogger<SqliteDatabaseProvider>> _loggerMock;
        private readonly SqliteDatabaseProvider _provider;

        public SqliteDatabaseProviderTests()
        {
            _applicationPathsMock = new Mock<IApplicationPaths>();
            _loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            _provider = new SqliteDatabaseProvider(_applicationPathsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Initialise_LogsConnectionString()
        {
            // Arrange
            var options = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions();

            // Act
            _provider.Initialise(options, databaseConfiguration);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void Initialise_LogsEnableSensitiveDataLogging()
        {
            // Arrange
            var options = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = bool.TrueString }
                    }
                }
            };

            // Act
            _provider.Initialise(options, databaseConfiguration);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
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
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContextMock.Object);
            _provider.DbContextFactory = dbContextFactoryMock.Object;

            // Act
            await _provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("jellyfin.db optimized successfully!")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
