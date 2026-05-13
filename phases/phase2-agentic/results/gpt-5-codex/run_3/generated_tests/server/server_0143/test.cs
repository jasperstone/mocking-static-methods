using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_ThrowsIfIntegrationFilterServiceMissing()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new Mock<IEventIntegrationPublisher>().Object);
            services.AddSingleton(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            services.AddSingleton(new Mock<IUserRepository>().Object);
            services.AddSingleton(new Mock<IOrganizationRepository>().Object);
            services.AddSingleton(new Mock<ILogger<EventIntegrationHandler<TestConfig>>>().Object);

            var listenerConfig = new TestListenerConfig();

            // Act
            var provider = services.BuildServiceProvider();
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                provider.GetRequiredService<IEventMessageHandler>();
            });

            // Assert
            Assert.Contains(nameof(IIntegrationFilterService), exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private class TestConfig { }

        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "test";
            public string IntegrationType => "test";
        }

        private interface IEventIntegrationPublisher { }

        private interface IIntegrationConfigurationDetailsCache { }

        private interface IUserRepository { }

        private interface IOrganizationRepository { }

        private interface IEventMessageHandler { }

        private interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
        }

        private class EventIntegrationHandler<TConfig> : IEventMessageHandler
        {
            public EventIntegrationHandler(
                string integrationType,
                IEventIntegrationPublisher eventIntegrationPublisher,
                object integrationFilterService,
                IIntegrationConfigurationDetailsCache configurationCache,
                IUserRepository userRepository,
                IOrganizationRepository organizationRepository,
                ILogger<EventIntegrationHandler<TConfig>> logger)
            {
            }
        }
    }
}
