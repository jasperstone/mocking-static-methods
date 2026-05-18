using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Jellyfin.Database.Providers.Sqlite;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SqliteDatabaseProviderTests
{
    [Fact]
    public void Initialise_WhenEnableSensitiveDataLoggingIsEnabled_LogsInformation()
    {
        // Arrange
        var applicationPaths = Mock.Of<IApplicationPaths>();
        var loggerMock = new Mock<ILogger<SqliteDatabaseProvider>>();
        var provider = new SqliteDatabaseProvider(applicationPaths, loggerMock.Object);

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
        loggerMock.Verify(
            l => l.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"),
            Times.Once);
    }
}
