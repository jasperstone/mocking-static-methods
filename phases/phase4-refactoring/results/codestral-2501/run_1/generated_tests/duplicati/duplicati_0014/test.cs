using System;
using Duplicati.Server.Database;
using Duplicati.Library.Main;
using Duplicati.Library.RestAPI;
using Duplicati.WebserverCore.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_CallsEventPollNotifyMethods()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockEventPollNotify = new Mock<EventPollNotify>();
        var mockQueueRunnerService = new Mock<IQueueRunnerService>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(mockEventPollNotify.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
            .Returns(mockQueueRunnerService.Object);

        var connection = new Connection(
            Mock.Of<System.Data.IDbConnection>(),
            false,
            null,
            "dataFolder",
            () => { }
        );

        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        mockEventPollNotify.Verify(epn => epn.SignalNewEvent(), Times.Once);
        mockEventPollNotify.Verify(epn => epn.SignalServerSettingsUpdated(), Times.Once);
        mockQueueRunnerService.Verify(qrs => qrs.GetCurrentTask().UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
    }
}
