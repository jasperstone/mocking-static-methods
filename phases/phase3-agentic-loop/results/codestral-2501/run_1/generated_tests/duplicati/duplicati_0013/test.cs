using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library.RestAPI;
using Duplicati.Server.Database;
using Duplicati.Server;

public class ConnectionTests
{
    [Fact]
    public void SignalSettingsChanged_ShouldCallGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockNotificationUpdateService = new Mock<INotificationUpdateService>();
        var mockEventPollNotify = new Mock<EventPollNotify>();
        var mockQueueRunnerService = new Mock<IQueueRunnerService>();
        var mockLiveControls = new Mock<LiveControls>();

        mockServiceProvider.Setup(sp => sp.GetRequiredService<INotificationUpdateService>()).Returns(mockNotificationUpdateService.Object);
        mockServiceProvider.Setup(sp => sp.GetRequiredService<EventPollNotify>()).Returns(mockEventPollNotify.Object);
        mockServiceProvider.Setup(sp => sp.GetRequiredService<IQueueRunnerService>()).Returns(mockQueueRunnerService.Object);
        mockServiceProvider.Setup(sp => sp.GetRequiredService<LiveControls>()).Returns(mockLiveControls.Object);

        var connection = new Connection(null, false, null, "", () => { });
        connection.SetServiceProvider(mockServiceProvider.Object);

        // Act
        connection.SignalSettingsChanged();

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService<INotificationUpdateService>(), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetRequiredService<EventPollNotify>(), Times.Exactly(2));
        mockServiceProvider.Verify(sp => sp.GetRequiredService<IQueueRunnerService>(), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetRequiredService<LiveControls>(), Times.Once);
    }
}
