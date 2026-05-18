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
    public void SignalSettingsChanged_CallsLiveControlsUpdatePowerModeProvider_WhenServiceProviderIsSet()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
            .Returns(() => new Mock<IDbCommand>().Object);
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>())
            .Returns(new Mock<LiveControls>().Object);
        
        var connection = new Connection(
            mockConnection.Object,
            disableFieldEncryption: true,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: () => { });

        connection.SetServiceProvider(mockServiceProvider.Object);

        var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        signalMethod.Invoke(connection, null);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
    }

    [Fact]
    public void SignalSettingsChanged_CallsGetRequiredServiceForEventPollNotify_Twice_WhenServiceProviderIsSet()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
            .Returns(() => new Mock<IDbCommand>().Object);
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(new Mock<EventPollNotify>().Object);
        
        var connection = new Connection(
            mockConnection.Object,
            disableFieldEncryption: true,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: () => { });

        connection.SetServiceProvider(mockServiceProvider.Object);

        var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        signalMethod.Invoke(connection, null);

        // Assert - called twice: SignalNewEvent and SignalServerSettingsUpdated
        mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(2));
    }

    [Fact]
    public void SignalSettingsChanged_DoesNothing_WhenServiceProviderIsNull()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
            .Returns(() => new Mock<IDbCommand>().Object);
        
        var connection = new Connection(
            mockConnection.Object,
            disableFieldEncryption: true,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: () => { });

        var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", 
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        // Act
        signalMethod.Invoke(connection, null);

        // Assert - no exceptions thrown, no service calls made
        mockConnection.VerifyAll();
    }

    [Fact]
    public void SetServiceProvider_CachesNotificationService()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>()))
            .Returns(() => new Mock<IDbCommand>().Object);
        
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(new Mock<EventPollNotify>().Object);
        
        var connection = new Connection(
            mockConnection.Object,
            disableFieldEncryption: true,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: () => { });

        // Act
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Assert - caches the services by calling GetRequiredService
        mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Once);
    }
}
