using System;
using System.Collections.ObjectModel;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.DbConfiguration;
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
        _mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
        _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
    }

    [Fact]
    public void Initialise_WithEnableSensitiveDataLoggingTrue_LogsEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder();
        var customOptions = new CustomDatabaseOptions
        {
            PluginName = "Test",
            PluginAssembly = "TestAssembly",
            ConnectionString = "test",
            Options = new Collection<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
            }
        };
        var databaseConfiguration = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite",
            CustomProviderOptions = customOptions
        };

        // Act
        _provider.Initialise(options, databaseConfiguration);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Initialise_WithEnableSensitiveDataLoggingFalse_DoesNotLogEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder();
        var customOptions = new CustomDatabaseOptions
        {
            PluginName = "Test",
            PluginAssembly = "TestAssembly",
            ConnectionString = "test",
            Options = new Collection<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "false" }
            }
        };
        var databaseConfiguration = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite",
            CustomProviderOptions = customOptions
        };

        // Act
        _provider.Initialise(options, databaseConfiguration);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Initialise_WithNoEnableSensitiveDataLoggingOption_DoesNotLogEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder();
        var databaseConfiguration = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite"
        };

        // Act
        _provider.Initialise(options, databaseConfiguration);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString()!.Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
