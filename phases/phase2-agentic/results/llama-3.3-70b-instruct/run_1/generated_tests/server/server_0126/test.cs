using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Bit.Core.Entities;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAzureServiceBusIntegration_EventIntegrationHandlerCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns(IntegrationType.Event);
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventIntegrationHandler = serviceProvider.GetService<IEventMessageHandler>();
            Assert.NotNull(eventIntegrationHandler);
        }

        [Fact]
        public async Task AddAzureServiceBusIntegration_AzureServiceBusEventListenerServiceCreated()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns(IntegrationType.Event);
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);

            var serviceBusService = new Mock<IAzureServiceBusService>();
            var loggerFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(serviceBusService.Object);
            services.AddSingleton(loggerFactory.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var azureServiceBusEventListenerService = serviceProvider.GetService<IHostedService>();
            Assert.NotNull(azureServiceBusEventListenerService);
        }
    }
}
