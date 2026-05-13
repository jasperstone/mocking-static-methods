using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddAzureServiceBusIntegration<MockConfig, MockListenerConfig>(listenerConfiguration.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var eventHandler = provider.GetRequiredService<IEventMessageHandler>();
            var eventListenerService = provider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventHandler);
            Assert.NotNull(eventListenerService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_UsesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var serviceProvider = services.BuildServiceProvider();

            // Mock dependencies
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<MockConfig>>>();

            services.AddSingleton(eventIntegrationPublisherMock.Object);
            services.AddSingleton(integrationFilterServiceMock.Object);
            services.AddSingleton(configurationCacheMock.Object);
            services.AddSingleton(userRepositoryMock.Object);
            services.AddSingleton(organizationRepositoryMock.Object);
            services.AddSingleton(loggerMock.Object);

            // Act
            services.AddAzureServiceBusIntegration<MockConfig, MockListenerConfig>(listenerConfiguration.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var eventHandler = provider.GetRequiredService<IEventMessageHandler>();

            eventHandler.HandleEventAsync(new EventIntegration());

            eventIntegrationPublisherMock.Verify(p => p.PublishEventAsync(It.IsAny<EventIntegration>()), Times.Once);
            integrationFilterServiceMock.Verify(s => s.ShouldProcessIntegration(It.IsAny<Integration>()), Times.Once);
            configurationCacheMock.Verify(c => c.GetConfigurationDetails(It.IsAny<string>()), Times.Once);
            userRepositoryMock.Verify(u => u.GetUserAsync(It.IsAny<string>()), Times.Once);
            organizationRepositoryMock.Verify(o => o.GetOrganizationAsync(It.IsAny<string>()), Times.Once);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }

    // Mock classes for testing
    public class MockConfig { }
    public class MockListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "mockKey";
        public string IntegrationType => "mockType";
        public int EventPrefetchCount => 1;
        public int EventMaxConcurrentCalls => 1;
        public int IntegrationPrefetchCount => 1;
        public int IntegrationMaxConcurrentCalls => 1;
    }
}
