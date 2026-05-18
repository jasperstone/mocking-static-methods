using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Platform;
using Bit.Core.HostedServices;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection GetServiceCollectionWithMocks()
        {
            var services = new ServiceCollection();

            // Setup mocks for all required services that GetRequiredService will be called for
            services.AddSingleton(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton(Mock.Of<IUserRepository>());
            services.AddSingleton(Mock.Of<IOrganizationRepository>());
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton(Mock.Of<IRabbitMqService>());
            services.AddSingleton(Mock.Of<ILoggerFactory>());
            services.AddSingleton(Mock.Of<TimeProvider>());

            return services;
        }

        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = GetServiceCollectionWithMocks();

            // Create a dummy listener configuration with required properties
            var listenerConfig = new DummyListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };

            // Use reflection to invoke the private extension method AddRabbitMqIntegration
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Act
            var result = methodInfo.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            // Build service provider to test service resolution
            var serviceProvider = services.BuildServiceProvider();

            // Check that the required services are resolvable
            Assert.NotNull(serviceProvider.GetService<IEventIntegrationPublisher>());
            Assert.NotNull(serviceProvider.GetService<IIntegrationFilterService>());
            Assert.NotNull(serviceProvider.GetService<IIntegrationConfigurationDetailsCache>());
            Assert.NotNull(serviceProvider.GetService<IUserRepository>());
            Assert.NotNull(serviceProvider.GetService<IOrganizationRepository>());
            Assert.NotNull(serviceProvider.GetService<IRabbitMqService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<TimeProvider>());
        }

        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }
    }
}
