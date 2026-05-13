using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListenerServices_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var provider = services.BuildServiceProvider();

            // Act
            services.AddEventIntegrationListenerServices<TConfig>(listenerConfiguration.Object);

            // Assert
            provider.GetRequiredService<IEventIntegrationPublisher>();
            provider.GetRequiredService<IIntegrationFilterService>();
            provider.GetRequiredService<IIntegrationConfigurationDetailsCache>();
            provider.GetRequiredService<IUserRepository>();
            provider.GetRequiredService<IOrganizationRepository>();
            provider.GetRequiredService<ILogger<EventIntegrationHandler<TConfig>>>();
        }

        [Fact]
        public void AddEventIntegrationListenerServices_GetRequiredKeyedService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var provider = services.BuildServiceProvider();

            // Act
            services.AddEventIntegrationListenerServices<TConfig>(listenerConfiguration.Object);

            // Assert
            provider.GetRequiredKeyedService<IEventMessageHandler>(listenerConfiguration.Object.RoutingKey);
        }

        [Fact]
        public void AddEventIntegrationListenerServices_GetRequiredService_TimeProvider_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var provider = services.BuildServiceProvider();

            // Act
            services.AddEventIntegrationListenerServices<TConfig>(listenerConfiguration.Object);

            // Assert
            provider.GetRequiredService<TimeProvider>();
        }
    }
}
