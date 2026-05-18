using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.HostedServices;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_Should_RegisterServicesAndHandlers()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockPublisher = new Mock<IEventIntegrationPublisher>();
            var mockFilterService = new Mock<IIntegrationFilterService>();
            var mockCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var mockAzureServiceBus = new Mock<IAzureServiceBusService>();
            var mockHandler = new Mock<IIntegrationHandler<object>>();
            var mockFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(mockPublisher.Object);
            services.AddSingleton(mockFilterService.Object);
            services.AddSingleton(mockCache.Object);
            services.AddSingleton(mockUserRepo.Object);
            services.AddSingleton(mockOrgRepo.Object);
            services.AddSingleton(mockLogger.Object);
            services.AddSingleton(mockAzureServiceBus.Object);
            services.AddSingleton(mockHandler.Object);
            services.AddSingleton(mockFactory.Object);

            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "test",
                IntegrationType = "type",
                EventPrefetchCount = 10,
                EventMaxConcurrentCalls = 5
            };

            // Act
            services.AddAzureServiceBusIntegration<SomeConfig, IDummyListenerConfig>(listenerConfig);

            var provider = services.BuildServiceProvider();

            // Assert
            var handlers = provider.GetServices<IKeyedService<IEventMessageHandler>>();
            Assert.NotNull(handlers);
            Assert.Contains(handlers, h => h.ServiceKey == "test");
        }

        // Dummy implementations for testing
        public class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }

        public class SomeConfig { }

        public interface IDummyListenerConfig : IIntegrationListenerConfiguration { }
    }
}
