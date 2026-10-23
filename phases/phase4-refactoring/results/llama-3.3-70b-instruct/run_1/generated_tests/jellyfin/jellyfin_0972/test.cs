using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database;
using Jellyfin.Database.Implementations;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Database.Providers.Sqlite.Tests
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
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task RunScheduledOptimisation_LogsOptimisationSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPathsMock = new Mock<IApplicationPaths>();
            var databaseConfiguration = new DatabaseConfigurationOptions();
            var options = new DbContextOptionsBuilder();
            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();

            // Act
            var provider = new SqliteDatabaseProvider(applicationPathsMock.Object, loggerMock.Object);
            provider.DbContextFactory = dbContextFactoryMock.Object;
            await provider.RunScheduledOptimisation(CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
