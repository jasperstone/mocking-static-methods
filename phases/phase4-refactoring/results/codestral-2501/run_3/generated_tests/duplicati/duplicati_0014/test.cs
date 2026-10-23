using System;
using Duplicati.Server.Database;
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
        var mockTask = new Mock<ITask>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<EventPollNotify>())
            .Returns(mockEventPollNotify.Object);

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService<IQueueRunnerService>())
            .Returns(mockQueueRunnerService.Object);

        mockQueueRunnerService
            .Setup(qrs => qrs.GetCurrentTask())
            .Returns(mockTask.Object);

        var connection = new Connection(
            Mock.Of<IDbConnection>(),
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
        mockTask.Verify(t => t.UpdateThrottleSpeeds(It.IsAny<long>(), It.IsAny<long>()), Times.Once);
    }
}
