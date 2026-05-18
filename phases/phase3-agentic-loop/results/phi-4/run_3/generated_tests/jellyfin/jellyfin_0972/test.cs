using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Database.Implementations.DbConfiguration;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.Database.Providers.Sqlite.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_WithEnableSensitiveDataLogging_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var applicationPaths = new Mock<IApplicationPaths>();
            var provider = new SqliteDatabaseProvider(applicationPaths.Object, mockLogger.Object);

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption("EnableSensitiveDataLogging", "true")
            };

            var databaseConfiguration = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = customOptions
                }
            };

            var optionsBuilder = new Mock<DbContextOptionsBuilder>();

            // Act
            provider.Initialise(optionsBuilder.Object, databaseConfiguration);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
