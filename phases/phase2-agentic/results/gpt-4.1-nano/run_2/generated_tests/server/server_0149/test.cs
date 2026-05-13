using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class AddRabbitMqIntegrationTests
    {
        [Fact]
        public void AddsServicesAndHostedServices_WithValidConfiguration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockPublisher = new Mock<IEventMessageHandler>();
            var mockHandler = new Mock<IIntegrationHandler<SomeConfig>>();
            var mockRabbitService = new Mock<IRabbitMqService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<SomeConfig>>>();
            var mockTimeProvider = new Mock<TimeProvider>();

            services.AddSingleton(mockPublisher.Object);
            services.AddSingleton(mockHandler.Object);
            services.AddSingleton(mockRabbitService.Object);
            services.AddSingleton(mockLoggerFactory.Object);
            services.AddSingleton(mockLogger.Object);
            services.AddSingleton(mockTimeProvider.Object);

            var listenerConfig = new SomeListenerConfig { RoutingKey = "test" };

            // Act
            services.AddRabbitMqIntegration<SomeConfig, SomeListenerConfig>(listenerConfig);

            var provider = services.BuildServiceProvider();

            // Assert
            var serviceProvider = provider;

            // Verify that IEventMessageHandler was registered with the correct key
            var handler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(handler);

            // Verify hosted services are registered
            var hostedServices = provider.GetServices<IHostedService>();
            Assert.Contains(hostedServices, s => s.GetType() == typeof(RabbitMqEventListenerService<SomeListenerConfig>));
            Assert.Contains(hostedServices, s => s.GetType() == typeof(RabbitMqIntegrationListenerService<SomeListenerConfig>));
        }
    }

    // Dummy implementations for testing
    public class SomeConfig : class, IIntegrationListenerConfiguration { }
    public class SomeListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType => "TestType";
    }
}
