using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_ValidServiceProvider_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<INotificationUpdateService>(Mock.Of<INotificationUpdateService>())
                .AddSingleton<EventPollNotify>(Mock.Of<EventPollNotify>())
                .BuildServiceProvider();

            var connection = new Connection { ServiceProvider = serviceProvider };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            var notificationUpdateService = serviceProvider.GetService<INotificationUpdateService>();
            Mock.Get(notificationUpdateService).Verify(s => s.IncrementLastDataUpdateId(), Times.Once);
        }

        [Fact]
        public void SignalSettingsChanged_NullServiceProvider_DoesNotCallGetRequiredService()
        {
            // Arrange
            var connection = new Connection { ServiceProvider = null };

            // Act
            connection.SignalSettingsChanged();

            // Assert
            // No exception is thrown
        }
    }

    public class Connection
    {
        public IServiceProvider ServiceProvider { get; set; }

        public void SignalSettingsChanged()
        {
            var provider = ServiceProvider;
            if (provider != null)
            {
                provider.GetRequiredService<INotificationUpdateService>()?.IncrementLastDataUpdateId();
                provider.GetRequiredService<EventPollNotify>()?.SignalNewEvent();
                provider.GetRequiredService<EventPollNotify>()?.SignalServerSettingsUpdated();
            }
        }
    }

    public interface INotificationUpdateService
    {
        void IncrementLastDataUpdateId();
    }

    public class EventPollNotify
    {
        public void SignalNewEvent() { }
        public void SignalServerSettingsUpdated() { }
    }
}
