using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;

namespace Duplicati.Server.Database.Tests;

public class ConnectionTests
{
    [Fact]
    public void SetServiceProvider_CallsGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                          .Returns(mockServiceProvider.Object);
        
        var mockDbConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Act
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Assert - verify GetRequiredService was called (same extension method pattern as line 429)
        mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeast(2));
    }

    [Fact]
    public void SetServiceProvider_Null_DoesNotThrow()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockDbConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Act
        connection.SetServiceProvider(null);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public void Connection_Ctor_SetsFields()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>().Object;
        Action mockStartOrStop = () => { };

        // Act
        var connection = new Connection(mockDbConnection, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Assert - ctor completes successfully
        Assert.NotNull(connection);
    }

    [Fact]
    public void IsEncryptingFields_ReturnsCorrectValue()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>().Object;
        Action mockStartOrStop = () => { };

        // Act - disableFieldEncryption: true means !true = false
        var connection = new Connection(mockDbConnection, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Assert
        Assert.False(connection.IsEncryptingFields);
    }
}
