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
            applicationPathsMock.Setup(ap => ap.DataPath).Returns("DataPath");
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            var options = new DbContextOptionsBuilder();
            var databaseConfiguration = new DatabaseConfigurationOptions();

            // Act
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
            applicationPathsMock.Setup(ap => ap.DataPath).Returns("DataPath");
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            var options = new DbContextOptionsBuilder();
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

            // Act
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
            applicationPathsMock.Setup(ap => ap.DataPath).Returns("DataPath");
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.DbContextFactory = new Mock<IDbContextFactory<JellyfinDbContext>>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await provider.RunScheduledOptimisation(cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogInformation("jellyfin.db optimized successfully!"), Times.Once);
        }
    }
}
