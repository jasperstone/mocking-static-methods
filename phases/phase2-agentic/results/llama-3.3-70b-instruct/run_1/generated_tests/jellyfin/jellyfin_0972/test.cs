using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Providers.Sqlite;

namespace Jellyfin.Database.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public async Task Initialise_LogsConnectionString()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions();
            var options = new DbContextOptionsBuilder();

            // Act
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.Initialise(options, databaseConfiguration);

            // Assert
            loggerMock.Verify(l => l.LogInformation("SQLite connection string: {ConnectionString}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Initialise_LogsEnableSensitiveDataLogging()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomDatabaseOptions
                {
                    Options = new List<CustomDatabaseOption>
                    {
                        new CustomDatabaseOption("EnableSensitiveDataLogging", "true")
                    }
                }
            };
            var options = new DbContextOptionsBuilder();

            // Act
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.Initialise(options, databaseConfiguration);

            // Assert
            loggerMock.Verify(l => l.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"), Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimisationSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var databaseConfiguration = new DatabaseConfigurationOptions();
            var options = new DbContextOptionsBuilder();

            // Act
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.DbContextFactory = dbContextFactoryMock.Object;
            await provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation("jellyfin.db optimized successfully!"), Times.Once);
        }
    }
}
