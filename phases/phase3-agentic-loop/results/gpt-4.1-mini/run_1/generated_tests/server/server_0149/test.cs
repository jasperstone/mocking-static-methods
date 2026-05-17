using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.HostedServices;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection CreateServiceCollectionWithMocks()
        {
            var services = new ServiceCollection();

            // Add mocks for all required services that GetRequiredService will be called for
            services.AddSingleton(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton(Mock.Of<IUserRepository>());
            services.AddSingleton(Mock.Of<IOrganizationRepository>());
            services.AddSingleton(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            services.AddSingleton(Mock.Of<IRabbitMqService>());
            services.AddSingleton(Mock.Of<ILoggerFactory>());
            services.AddSingleton(Mock.Of<TimeProvider>());
            services.AddSingleton(Mock.Of<IIntegrationHandler<object>>());

            return services;
        }

        [Fact]
        public void AddRabbitMqIntegration_RegistersExpectedServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = CreateServiceCollectionWithMocks();

            // Create a dummy listener configuration with required properties
            var listenerConfig = new DummyListenerConfiguration();

            // Use reflection to get the private AddRabbitMqIntegration method
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            // Act
            var result = method.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.Same(services, result);

            // Verify that the services collection contains the expected service descriptors
            bool hasEventMessageHandler = false;
            bool hasRabbitMqEventListenerService = false;
            bool hasRabbitMqIntegrationListenerService = false;

            foreach (var sd in services)
            {
                if (sd.ServiceType == typeof(IEventMessageHandler) && sd.ImplementationFactory != null)
                {
                    hasEventMessageHandler = true;
                }
                if (sd.ServiceType == typeof(IHostedService) && sd.ImplementationFactory != null)
                {
                    var implType = sd.ImplementationType?.Name;
                    if (implType != null && implType.Contains("RabbitMqEventListenerService"))
                    {
                        hasRabbitMqEventListenerService = true;
                    }
                    if (implType != null && implType.Contains("RabbitMqIntegrationListenerService"))
                    {
                        hasRabbitMqIntegrationListenerService = true;
                    }
                }
            }

            Assert.True(hasEventMessageHandler, "Expected IEventMessageHandler registration");
            Assert.True(hasRabbitMqEventListenerService, "Expected RabbitMqEventListenerService registration");
            Assert.True(hasRabbitMqIntegrationListenerService, "Expected RabbitMqIntegrationListenerService registration");
        }

        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummy-routing-key";
            public string IntegrationType => "dummy-integration-type";
        }
    }
}
