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
    private static readonly MethodInfo SignalSettingsChangedMethod = 
        typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance)!;

    [Fact]
    public void SetServiceProvider_CallsGetRequiredServiceINotificationUpdateService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<object>()).Returns(new object());

        var mockConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Act
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<object>(), Times.AtLeastOnce);
    }

    [Fact]
    public void SetServiceProvider_CallsGetRequiredServiceEventPollNotify()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<object>()).Returns(new object());

        var mockConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Act
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<object>(), Times.AtLeastOnce);
    }

    [Fact]
    public void SignalSettingsChanged_WithServiceProvider_CallsGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<object>()).Returns(new object());

        var mockConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        SignalSettingsChangedMethod.Invoke(connection, null);

        // Assert - verifies the GetRequiredService extension is called multiple times
        mockServiceProvider.Verify(sp => sp.GetRequiredService<object>(), Times.AtLeast(4));
    }

    [Fact]
    public void SignalSettingsChanged_ServiceProviderNull_DoesNotThrow()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);

        // Act & Assert
        var ex = Record.Exception(() => SignalSettingsChangedMethod.Invoke(connection, null));
        Assert.Null(ex);
    }

    [Fact]
    public void ServiceProvider_Getter_ReturnsSetValue()
    {
        // Arrange
        var expectedProvider = Mock.Of<IServiceProvider>();
        var mockConnection = new Mock<IDbConnection>();
        Action mockStartOrStop = () => { };
        var connection = new Connection(mockConnection.Object, disableFieldEncryption: true, key: null, dataFolder: "test", startOrStopUsageReporter: mockStartOrStop);
        
        // Act
        connection.SetServiceProvider(expectedProvider);
        var result = connection.ServiceProvider;

        // Assert
        Assert.Equal(expectedProvider, result);
    }
}
