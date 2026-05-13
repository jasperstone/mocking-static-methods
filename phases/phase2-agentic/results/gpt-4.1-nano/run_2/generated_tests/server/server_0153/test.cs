using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void Test_RabbitMqIntegrationListenerService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockHandler = new Mock<IIntegrationHandler<object>>();
            var mockTimeProvider = new Mock<TimeProvider>();

            // Setup mocks for GetRequiredService calls
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationHandler<object>>()).Returns(mockHandler.Object);
            mockProvider.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(mockRabbitMqService.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockProvider.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(mockTimeProvider.Object);

            // Setup for GetRequiredKeyedService
            var routingKey = "testKey";

            // Act
            services.AddLogging(); // To ensure ILoggerFactory is available
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService,
                RabbitMqIntegrationListenerService<object>>(provider =>
                    new RabbitMqIntegrationListenerService<object>(
                        handler: provider.GetRequiredService<IIntegrationHandler<object>>(),
                        configuration: new { RoutingKey = routingKey },
                        rabbitMqService: provider.GetRequiredService<IRabbitMqService>(),
                        loggerFactory: provider.GetRequiredService<ILoggerFactory>(),
                        timeProvider: provider.GetRequiredService<TimeProvider>()
                    )
                )
            );

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var hostedServices = serviceProvider.GetServices<IHostedService>();
            Assert.NotEmpty(hostedServices);
        }
    }
}
