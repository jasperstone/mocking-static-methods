using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Reflection;
using Duplicati.Server.Database;
using Duplicati.Server.Serialization.Interface;

namespace Duplicati.Server.Database.Tests;

public class ConnectionTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Action _startOrStopUsageReporter;

    public ConnectionTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockConnection.Setup(c => c.CreateCommand(It.IsAny<string>())).Returns(new Mock<IDbCommand>().Object);
        _startOrStopUsageReporter = () => { };
    }

    [Fact]
    public void SignalSettingsChanged_WithServiceProvider_CallsGetRequiredService_EventPollNotify_Twice()
    {
        // Arrange
        var mockProvider = new Mock<IServiceProvider>();
        var mockEventPollNotify = new Mock<EventPollNotify>();
        mockProvider.Setup(x => x.GetService(typeof(EventPollNotify))).Returns(mockEventPollNotify.Object);
        mockProvider.Setup(x => x.GetService(typeof(INotificationUpdateService))).Returns(new Mock<INotificationUpdateService>().Object);
        mockProvider.Setup(x => x.GetService(typeof(IQueueRunnerService))).Returns(new Mock<IQueueRunnerService>().Object);
        mockProvider.Setup(x => x.GetService(typeof(LiveControls))).Returns(new Mock<LiveControls>().Object);

        var connection = new Connection(
            _mockConnection.Object,
            disableFieldEncryption: false,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: _startOrStopUsageReporter);

        // Set private m_serviceProvider field via reflection
        var serviceProviderField = typeof(Connection).GetField("m_serviceProvider", BindingFlags.NonPublic | BindingFlags.Instance);
        serviceProviderField?.SetValue(connection, mockProvider.Object);

        // Act
        var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        signalMethod?.Invoke(connection, null);

        // Assert
        mockProvider.Verify(x => x.GetService(typeof(EventPollNotify)), Times.Exactly(2));
    }

    [Fact]
    public void SignalSettingsChanged_ServiceProviderNull_ExecutesWithoutException()
    {
        // Arrange
        var connection = new Connection(
            _mockConnection.Object,
            disableFieldEncryption: false,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: _startOrStopUsageReporter);

        // Act
        var signalMethod = typeof(Connection).GetMethod("SignalSettingsChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        var ex = Record.Exception(() => signalMethod?.Invoke(connection, null));

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void SetServiceProvider_CallsGetRequiredService_Twice()
    {
        // Arrange
        var mockProvider = new Mock<IServiceProvider>();
        mockProvider.Setup(x => x.GetService(typeof(INotificationUpdateService))).Returns(new Mock<INotificationUpdateService>().Object);
        mockProvider.Setup(x => x.GetService(typeof(EventPollNotify))).Returns(new Mock<EventPollNotify>().Object);

        var connection = new Connection(
            _mockConnection.Object,
            disableFieldEncryption: false,
            key: null,
            dataFolder: "test",
            startOrStopUsageReporter: _startOrStopUsageReporter);

        // Act
        connection.SetServiceProvider(mockProvider.Object);

        // Assert
        mockProvider.Verify(x => x.GetService(typeof(INotificationUpdateService)), Times.Once());
        mockProvider.Verify(x => x.GetService(typeof(EventPollNotify)), Times.Once());
    }
}
