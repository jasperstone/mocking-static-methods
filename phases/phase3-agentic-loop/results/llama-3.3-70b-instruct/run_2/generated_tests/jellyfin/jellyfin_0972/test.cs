using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        private readonly Mock<ILogger<SqliteDatabaseProvider>> _loggerMock;
        private readonly Mock<IApplicationPaths> _applicationPathsMock;

        public SqliteDatabaseProviderTests()
        {
            _loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            _applicationPathsMock = new Mock<IApplicationPaths>();
        }

        [Fact]
        public async Task Initialise_LogsConnectionString()
        {
            // Arrange
            var options = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = MediaBrowser.Common.Configuration.DatabaseType.Sqlite,
                CustomProviderOptions = new CustomDatabaseOptions
                {
                    PluginName = "Jellyfin-SQLite",
                    PluginAssembly = "Jellyfin.Database.Providers.Sqlite",
                    ConnectionString = "Data Source=jellyfin.db"
                }
            };
            var sqliteConnectionBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "jellyfin.db",
                Cache = SqliteCacheMode.Default,
                Pooling = true,
                DefaultTimeout = 60
            };
            var connectionString = sqliteConnectionBuilder.ToString();

            // Act
            var provider = new SqliteDatabaseProvider(_applicationPathsMock.Object, _loggerMock.Object);
            provider.Initialise(options, databaseConfiguration);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("SQLite connection string: {ConnectionString}", connectionString), Times.Once);
        }

        [Fact]
        public async Task Initialise_LogsEnableSensitiveDataLogging()
        {
            // Arrange
            var options = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                DatabaseType = MediaBrowser.Common.Configuration.DatabaseType.Sqlite,
                CustomProviderOptions = new CustomDatabaseOptions
                {
                    PluginName = "Jellyfin-SQLite",
                    PluginAssembly = "Jellyfin.Database.Providers.Sqlite",
                    ConnectionString = "Data Source=jellyfin.db",
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption("EnableSensitiveDataLogging", "true")
                    }
                }
            };

            // Act
            var provider = new SqliteDatabaseProvider(_applicationPathsMock.Object, _loggerMock.Object);
            provider.Initialise(options, databaseConfiguration);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"), Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimisationSuccess()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var context = new Mock<JellyfinDbContext>();
            context.Setup(c => c.Database.ExecuteSqlRawAsync(It.IsAny<string>(), cancellationToken)).ReturnsAsync(0);
            var provider = new SqliteDatabaseProvider(_applicationPathsMock.Object, _loggerMock.Object);
            provider.DbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>().Setup(f => f.CreateDbContextAsync(cancellationToken)).ReturnsAsync(context.Object);

            // Act
            await provider.RunScheduledOptimisation(cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("jellyfin.db optimized successfully!"), Times.Once);
        }
    }
}
