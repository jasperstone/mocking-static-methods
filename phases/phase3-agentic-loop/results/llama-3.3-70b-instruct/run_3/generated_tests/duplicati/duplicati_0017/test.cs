using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Duplicati.Library;
using Duplicati.Library.RestAPI;
using System;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredService_INotificationUpdateService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var notificationUpdateService = new Mock<INotificationUpdateService>();
            serviceProvider = new ServiceCollection()
                .AddSingleton<INotificationUpdateService>(notificationUpdateService.Object)
                .BuildServiceProvider();

            var connection = new Duplicati.Library.RestAPI.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            notificationUpdateService.Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_EventPollNotify()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var eventPollNotify = new Mock<Duplicati.Library.EventPollNotify>();
            serviceProvider = new ServiceCollection()
                .AddSingleton<Duplicati.Library.EventPollNotify>(eventPollNotify.Object)
                .BuildServiceProvider();

            var connection = new Duplicati.Library.RestAPI.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            eventPollNotify.Verify(e => e.SignalNewEvent(), Times.Once);
            eventPollNotify.Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_LiveControls()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var liveControls = new Mock<Duplicati.Library.LiveControls>();
            serviceProvider = new ServiceCollection()
                .AddSingleton<Duplicati.Library.LiveControls>(liveControls.Object)
                .BuildServiceProvider();

            var connection = new Duplicati.Library.RestAPI.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            liveControls.Verify(l => l.UpdatePowerModeProvider(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_GetRequiredService_IQueueRunnerService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var queueRunnerService = new Mock<Duplicati.Library.IQueueRunnerService>();
            serviceProvider = new ServiceCollection()
                .AddSingleton<Duplicati.Library.IQueueRunnerService>(queueRunnerService.Object)
                .BuildServiceProvider();

            var connection = new Duplicati.Library.RestAPI.Database.Connection(null, false, null, "", () => { });
            connection.SetServiceProvider(serviceProvider);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            queueRunnerService.Verify(q => q.GetCurrentTask(), Times.Once);
        }
    }
}
