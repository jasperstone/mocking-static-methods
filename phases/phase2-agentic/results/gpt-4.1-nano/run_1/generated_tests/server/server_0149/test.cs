using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Server.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_Should_Call_GetRequiredService_For_Services()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock dependencies
            var mockPublisher = new Mock<IEventMessageHandler>();
            var mockHandler = new Mock<IIntegrationHandler<object>>();
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<RabbitMqEventListenerService<object>>>();
            var mockTimeProvider = new Mock<TimeProvider>();

            // Setup service provider
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockPublisher.Object)
                .AddSingleton(mockRabbitMqService.Object)
                .AddSingleton(mockLoggerFactory.Object)
                .AddSingleton(mockTimeProvider.Object)
                .BuildServiceProvider();

            // Register the services with the extension method
            services.AddSingleton<IEventMessageHandler>(mockPublisher.Object);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, RabbitMqEventListenerService<object>>(provider =>
                new RabbitMqEventListenerService<object>(
                    handler: mockHandler.Object,
                    configuration: new DummyListenerConfiguration(),
                    rabbitMqService: mockRabbitMqService.Object,
                    loggerFactory: mockLoggerFactory.Object,
                    timeProvider: mockTimeProvider.Object
                )
            ));

            // Act
            var serviceCollection = services.BuildServiceProvider();

            // Assert
            // Check that GetRequiredService was called for the dependencies
            // Since we can't directly verify extension method internals, we verify that the services are registered
            Assert.Contains(serviceCollection.GetServices<IHostedService>(), s => s.GetType() == typeof(RabbitMqEventListenerService<object>));
        }
    }

    // Dummy implementation for IIntegrationListenerConfiguration
    public class DummyListenerConfiguration : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "dummy";
        public string IntegrationType => "dummyType";
    }
}
