using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Providers.Sqlite;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Database.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimizationSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var sqliteDatabaseProvider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            sqliteDatabaseProvider.DbContextFactory = dbContextFactoryMock.Object;

            // Act
            await sqliteDatabaseProvider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("jellyfin.db optimized successfully!"), Times.Once);
        }

        [Fact]
        public void Initialise_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var dbContextOptionsBuilderMock = new Mock<DbContextOptionsBuilder>();
            var sqliteDatabaseProvider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);

            // Act
            sqliteDatabaseProvider.Initialise(dbContextOptionsBuilderMock.Object, new DatabaseConfigurationOptions());

            // Assert
            loggerMock.Verify(l => l.LogInformation("SQLite connection string: {ConnectionString}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Initialise_LogsEnableSensitiveDataLogging()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var dbContextOptionsBuilderMock = new Mock<DbContextOptionsBuilder>();
            var sqliteDatabaseProvider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption("EnableSensitiveDataLogging", "true")
            };

            // Act
            sqliteDatabaseProvider.Initialise(dbContextOptionsBuilderMock.Object, new DatabaseConfigurationOptions { CustomProviderOptions = new CustomDatabaseOptions { Options = customOptions } });

            // Assert
            loggerMock.Verify(l => l.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"), Times.Once);
        }
    }
}
