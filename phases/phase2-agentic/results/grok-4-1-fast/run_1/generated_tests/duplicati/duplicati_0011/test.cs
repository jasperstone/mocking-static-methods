using Xunit;
using Moq;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Server.Database;
using Duplicati.Server.Serialization.Interface;
using Duplicati.WebserverCore.Abstractions;

namespace Duplicati.Server.Database.Tests;

public class ConnectionTests
{
    [Fact]
    public void SetServiceProvider_NullServiceProvider_DoesNotThrow()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        var mockDbCommand = new Mock<IDbCommand>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);
        
        var connection = new Connection(
            mockConnection.Object, 
            disableFieldEncryption: false, 
            key: null, 
            dataFolder: "test", 
            startOrStopUsageReporter: () => { });

        // Act & Assert
        Assert.DoesNotThrow(() => connection.SetServiceProvider(null));
    }

    [Fact]
    public void SetServiceProvider_ValidServiceProvider_SetsNotificationUpdateService()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationUpdateService>();
        var mockEventPollNotify = new Mock<EventPollNotify>();
        var services = new ServiceCollection();
        services.AddSingleton(mockNotificationService.Object);
        services.AddSingleton(mockEventPollNotify.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockConnection = new Mock<IDbConnection>();
        var mockDbCommand = new Mock<IDbCommand>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);
        
        var connection = new Connection(
            mockConnection.Object, 
            disableFieldEncryption: false, 
            key: null, 
            dataFolder: "test", 
            startOrStopUsageReporter: () => { });

        // Act
        connection.SetServiceProvider(serviceProvider);

        // Assert
        Assert.NotNull(connection.ServiceProvider);
        Assert.Same(mockNotificationService.Object, connection.GetType()
            .GetField("m_notificationUpdateService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(connection));
    }

    [Fact]
    public void SetServiceProvider_MissingNotificationService_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockEventPollNotify = new Mock<EventPollNotify>();
        var services = new ServiceCollection();
        services.AddSingleton(mockEventPollNotify.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockConnection = new Mock<IDbConnection>();
        var mockDbCommand = new Mock<IDbCommand>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);
        
        var connection = new Connection(
            mockConnection.Object, 
            disableFieldEncryption: false, 
            key: null, 
            dataFolder: "test", 
            startOrStopUsageReporter: () => { });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => connection.SetServiceProvider(serviceProvider));
    }

    [Fact]
    public void SetServiceProvider_MissingEventPollNotify_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationUpdateService>();
        var services = new ServiceCollection();
        services.AddSingleton(mockNotificationService.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockConnection = new Mock<IDbConnection>();
        var mockDbCommand = new Mock<IDbCommand>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);
        
        var connection = new Connection(
            mockConnection.Object, 
            disableFieldEncryption: false, 
            key: null, 
            dataFolder: "test", 
            startOrStopUsageReporter: () => { });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => connection.SetServiceProvider(serviceProvider));
    }

    [Fact]
    public void SetServiceProvider_CompleteServiceProvider_SetsBothServices()
    {
        // Arrange
        var mockNotificationService = new Mock<INotificationUpdateService>();
        var mockEventPollNotify = new Mock<EventPollNotify>();
        var services = new ServiceCollection();
        services.AddSingleton(mockNotificationService.Object);
        services.AddSingleton(mockEventPollNotify.Object);
        var serviceProvider = services.BuildServiceProvider();

        var mockConnection = new Mock<IDbConnection>();
        var mockDbCommand = new Mock<IDbCommand>();
        mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(mockDbCommand.Object);
        
        var connection = new Connection(
            mockConnection.Object, 
            disableFieldEncryption: false, 
            key: null, 
            dataFolder: "test", 
            startOrStopUsageReporter: () => { });

        // Act
        connection.SetServiceProvider(serviceProvider);

        // Assert - verify both private fields are set via reflection
        var notificationField = connection.GetType()
            .GetField("m_notificationUpdateService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventField = connection.GetType()
            .GetField("m_eventPollNotifyer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.NotNull(notificationField);
        Assert.NotNull(eventField);
        Assert.Same(mockNotificationService.Object, notificationField.GetValue(connection));
        Assert.Same(mockEventPollNotify.Object, eventField.GetValue(connection));
    }
}
