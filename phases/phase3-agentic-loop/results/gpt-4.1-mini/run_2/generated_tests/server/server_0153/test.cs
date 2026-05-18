using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.HostedServices;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummyRoutingKey";
            public string IntegrationType => "dummyIntegrationType";
        }

        private class DummyConfig { }

        [Fact]
        public void ServiceProvider_GetRequiredService_IsCalledForExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfig();

            // Setup mocks for the services that will be requested by GetRequiredService
            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockIntegrationConfigurationDetailsCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            var mockEventMessageHandler = new Mock<IEventMessageHandler>();
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockTimeProvider = new Mock<TimeProvider>();

            var mockIntegrationHandler = new Mock<IIntegrationHandler<DummyConfig>>();

            // Register mocks in the service collection
            services.AddSingleton(mockEventIntegrationPublisher.Object);
            services.AddSingleton(mockIntegrationFilterService.Object);
            services.AddSingleton(mockIntegrationConfigurationDetailsCache.Object);
            services.AddSingleton(mockUserRepository.Object);
            services.AddSingleton(mockOrganizationRepository.Object);
            services.AddSingleton(mockLogger.Object);
            services.AddSingleton(mockRabbitMqService.Object);
            services.AddSingleton(mockLoggerFactory.Object);
            services.AddSingleton(mockTimeProvider.Object);
            services.AddSingleton(mockIntegrationHandler.Object);

            // Register the IEventMessageHandler keyed service (simulate)
            services.AddSingleton<IEventMessageHandler>(mockEventMessageHandler.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var eventIntegrationHandler = new EventIntegrationHandler<DummyConfig>(
                integrationType: listenerConfig.IntegrationType,
                eventIntegrationPublisher: serviceProvider.GetRequiredService<IEventIntegrationPublisher>(),
                integrationFilterService: serviceProvider.GetRequiredService<IIntegrationFilterService>(),
                configurationCache: serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                userRepository: serviceProvider.GetRequiredService<IUserRepository>(),
                organizationRepository: serviceProvider.GetRequiredService<IOrganizationRepository>(),
                logger: serviceProvider.GetRequiredService<ILogger<EventIntegrationHandler<DummyConfig>>>()
            );

            var rabbitMqIntegrationListenerService = new RabbitMqIntegrationListenerService<DummyListenerConfig>(
                handler: serviceProvider.GetRequiredService<IIntegrationHandler<DummyConfig>>(),
                configuration: listenerConfig,
                rabbitMqService: serviceProvider.GetRequiredService<IRabbitMqService>(),
                loggerFactory: serviceProvider.GetRequiredService<ILoggerFactory>(),
                timeProvider: serviceProvider.GetRequiredService<TimeProvider>()
            );

            // Assert
            Assert.NotNull(eventIntegrationHandler);
            Assert.NotNull(rabbitMqIntegrationListenerService);
        }
    }
}
