using System;
using System.Collections.ObjectModel;
using Jellyfin.Database.Providers.Sqlite;
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
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
    }

    [Fact]
    public void Initialise_EnablesSensitiveDataLogging_LogsEnableMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var customOptions = new Collection<CustomDatabaseOption>
        {
            new() { Key = "EnableSensitiveDataLogging", Value = "true" }
        };
        var databaseOptions = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite",
            CustomProviderOptions = new CustomDatabaseOptions
            {
                PluginName = "test",
                PluginAssembly = "test",
                ConnectionString = "test",
                Options = customOptions
            }
        };

        // Act
        _provider.Initialise(optionsBuilder, databaseOptions);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Initialise_NoSensitiveDataLoggingOption_DoesNotLogEnableMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var databaseOptions = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite"
        };

        // Act
        _provider.Initialise(optionsBuilder, databaseOptions);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void Initialise_SensitiveDataLoggingDisabled_DoesNotLogEnableMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var customOptions = new Collection<CustomDatabaseOption>
        {
            new() { Key = "EnableSensitiveDataLogging", Value = "false" }
        };
        var databaseOptions = new DatabaseConfigurationOptions
        {
            DatabaseType = "SQLite",
            CustomProviderOptions = new CustomDatabaseOptions
            {
                PluginName = "test",
                PluginAssembly = "test",
                ConnectionString = "test",
                Options = customOptions
            }
        };

        // Act
        _provider.Initialise(optionsBuilder, databaseOptions);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(state => state.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
