using Xunit;
using Moq;
using Duplicati.Library;
using Duplicati.Library.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_IncrementLastDataUpdateId()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<INotificationUpdateService>(Mock.Of<INotificationUpdateService>())
                .BuildServiceProvider();

            var connection = new Connection { ServiceProvider = serviceProvider };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            var notificationUpdateService = serviceProvider.GetService<INotificationUpdateService>();
            Mock.Get(notificationUpdateService).Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_SignalNewEvent()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<EventPollNotify>(Mock.Of<EventPollNotify>())
                .BuildServiceProvider();

            var connection = new Connection { ServiceProvider = serviceProvider };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            var eventPollNotify = serviceProvider.GetService<EventPollNotify>();
            Mock.Get(eventPollNotify).Verify(e => e.SignalNewEvent(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_SignalServerSettingsUpdated()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<EventPollNotify>(Mock.Of<EventPollNotify>())
                .BuildServiceProvider();

            var connection = new Connection { ServiceProvider = serviceProvider };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            var eventPollNotify = serviceProvider.GetService<EventPollNotify>();
            Mock.Get(eventPollNotify).Verify(e => e.SignalServerSettingsUpdated(), Times.Once);
        }
    }
}
