using System;
using System.Collections.Generic;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Providers.Sqlite;
using MediaBrowser.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Extensions;
using Xunit;

namespace Jellyfin.Database.Tests.Providers.Sqlite;

public class SqliteDatabaseProviderTests
{
    private readonly Mock<IApplicationPaths> _mockApplicationPaths;
    private readonly Mock<ILogger<SqliteDatabaseProvider>> _mockLogger;
    private readonly SqliteDatabaseProvider _provider;

    public SqliteDatabaseProviderTests()
    {
        _mockApplicationPaths = new Mock<IApplicationPaths>();
        _mockApplicationPaths.Setup(x => x.DataPath).Returns("/tmp/data");
        _mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
        _mockLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        _provider = new SqliteDatabaseProvider(_mockApplicationPaths.Object, _mockLogger.Object);
    }

    [Fact]
    public void Initialise_WithEnableSensitiveDataLoggingTrue_LogsEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var customOptions = new List<CustomDatabaseOption>
        {
            new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
        };
        var databaseConfig = new DatabaseConfigurationOptions
        {
            CustomProviderOptions = new CustomDatabaseProviderOptions { Options = customOptions }
        };

        // Act
        _provider.Initialise(optionsBuilder, databaseConfig);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"),
            Times.Once);
    }

    [Fact]
    public void Initialise_WithEnableSensitiveDataLoggingFalse_DoesNotLogEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var customOptions = new List<CustomDatabaseOption>
        {
            new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "false" }
        };
        var databaseConfig = new DatabaseConfigurationOptions
        {
            CustomProviderOptions = new CustomDatabaseProviderOptions { Options = customOptions }
        };

        // Act
        _provider.Initialise(optionsBuilder, databaseConfig);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"),
            Times.Never);
    }

    [Fact]
    public void Initialise_WithNoEnableSensitiveDataLoggingOption_DoesNotLogEnableSensitiveDataLoggingMessage()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var databaseConfig = new DatabaseConfigurationOptions();

        // Act
        _provider.Initialise(optionsBuilder, databaseConfig);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation("EnableSensitiveDataLogging is enabled on SQLite connection"),
            Times.Never);
    }

    [Fact]
    public void Initialise_Always_LogsConnectionString()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        var databaseConfig = new DatabaseConfigurationOptions();

        // Act
        _provider.Initialise(optionsBuilder, databaseConfig);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation("SQLite connection string: {ConnectionString}", It.IsAny<object[]>()),
            Times.Once);
    }
}
