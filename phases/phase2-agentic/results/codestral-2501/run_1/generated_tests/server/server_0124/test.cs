using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Vault;
using Microsoft.Extensions.Logging;
using System;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterEventMessageHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testType");
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(5);

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var azureServiceBusService = new Mock<IAzureServiceBusService>();
            var loggerFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);
            services.AddSingleton(azureServiceBusService.Object);
            services.AddSingleton(loggerFactory.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var handler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(handler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterHostedService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testType");
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(5);

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var azureServiceBusService = new Mock<IAzureServiceBusService>();
            var loggerFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);
            services.AddSingleton(azureServiceBusService.Object);
            services.AddSingleton(loggerFactory.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.NotNull(hostedService);
        }
    }
}
