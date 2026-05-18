using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using Duplicati.Server.Database;
using System.Reflection;

namespace Duplicati.Server.Database.Tests;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_WithServiceProvider_CallsGetRequiredServiceMultipleTimes()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(new object());

        var mockDbConnection = new Mock<IDbConnection>().Object;

        var connection = new Connection(
            mockDbConnection,
            false,
            null,
            "test",
            () => { }
        );

        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        InvokeSignalSettingsChanged(connection);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.AtLeast(4));
    }

    [Fact]
    public void SignalSettingsChanged_ServiceProviderNull_DoesNotThrow()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>().Object;
        var connection = new Connection(
            mockDbConnection,
            false,
            null,
            "test",
            () => { }
        );

        // Act & Assert
        var ex = Record.Exception(() => InvokeSignalSettingsChanged(connection));
        Assert.Null(ex);
    }

    [Fact]
    public void SetServiceProvider_CallsGetRequiredService_Twice()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Returns(new object());

        var mockDbConnection = new Mock<IDbConnection>().Object;
        var connection = new Connection(
            mockDbConnection,
            false,
            null,
            "test",
            () => { }
        );

        // Act
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Exactly(2));
    }

    private static void InvokeSignalSettingsChanged(Connection connection)
    {
        var method = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(connection, null);
    }
}
