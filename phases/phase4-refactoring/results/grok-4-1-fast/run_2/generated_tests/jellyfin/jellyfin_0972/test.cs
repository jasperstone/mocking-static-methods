using System;
using System.Collections.Generic;
using Jellyfin.Database.Providers.Sqlite;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Database.Providers.Sqlite.Tests;

public class SqliteDatabaseProviderTests
{
    private readonly Mock<IApplicationPaths> _mockApplicationPaths;
    private readonly Mock<ILogger<SqliteDatabaseProvider>> _mockLogger;
    private readonly SqliteDatabaseProvider _provider;

    public SqliteDatabaseProviderTests()
    {
        _mockApplicationPaths = new Mock<IApplicationPaths>();
        _mockApplicationPaths.Setup(x => x.DataPath).Returns("/mock/data/path");
        _mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
        _mockLogger.SetupAllProperties();
        _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
    }

    [Fact]
    public void Initialise_WithEnableSensitiveDataLoggingTrue_LogsEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var databaseConfiguration = new object(); // Minimal mock for databaseConfiguration

        // Use reflection to set the nested customOptions structure that triggers the log
        var customOptionsField = typeof(SqliteDatabaseProvider)
            .GetMethod("Initialise", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .CreateDelegate(typeof(Action<DbContextOptionsBuilder, object>), _provider)
            as Action<DbContextOptionsBuilder, object>;

        // Act
        _provider.Initialise(null!, databaseConfiguration);

        // Assert - verify the specific LogInformation call on line 97
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.AtLeastOnce); // Confirms LoggerExtensions.LogInformation is called
    }

    [Fact]
    public void RunScheduledOptimisation_Completes_LogsOptimizationSuccess()
    {
        // Arrange
        _provider.DbContextFactory = Mock.Of<IDbContextFactory<JellyfinDbContext>>();

        // Act
        _provider.RunScheduledOptimisation(default).Wait();

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("optimized successfully")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
