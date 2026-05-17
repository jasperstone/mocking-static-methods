using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_LogsConnectionString()
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
        public void Initialise_LogsEnableSensitiveDataLogging()
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
            var contextMock = new Mock<JellyfinDbContext>();
            var database = contextMock.Object.Database;
            database.Setup(d => d.ExecuteSqlRawAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

            dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(contextMock.Object);

            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.DbContextFactory = dbContextFactoryMock.Object;

            // Act
            await provider.RunScheduledOptimisation(default);

            // Assert
            loggerMock.Verify(l => l.LogInformation("jellyfin.db optimized successfully!"), Times.Once);
        }
    }
}
