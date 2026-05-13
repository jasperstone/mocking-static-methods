using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class AddRabbitMqIntegrationTests
    {
        [Fact]
        public void AddsRabbitMqServices_WithValidConfiguration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(c => c.RoutingKey).Returns("testRoutingKey");
            listenerConfiguration.SetupGet(c => c.IntegrationType).Returns("TestType");

            var mockHandler = new Mock<IEventMessageHandler>();
            var mockHandlerFactory = new Func<IServiceProvider, object, IEventMessageHandler>((sp, _) => mockHandler.Object);

            var mockHandlerProvider = new Mock<IServiceProvider>();
            mockHandlerProvider.Setup(sp => sp.GetRequiredKeyedService<IEventMessageHandler>("testRoutingKey"))
                .Returns(mockHandler.Object);

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockTimeProvider = new Mock<TimeProvider>();

            // Act
            services.TryAddKeyedSingleton<IEventMessageHandler>(serviceKey: "testRoutingKey", implementationFactory: (provider, _) =>
            {
                // Simulate resolving dependencies
                var handler = provider.GetRequiredService<IEventMessageHandler>();
                return handler;
            });

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService,
                RabbitMqEventListenerService<Mock>>((provider) =>
                    new RabbitMqEventListenerService<Mock>(
                        handler: provider.GetRequiredKeyedService<IEventMessageHandler>("testRoutingKey"),
                        configuration: listenerConfiguration.Object,
                        rabbitMqService: mockRabbitMqService.Object,
                        loggerFactory: mockLoggerFactory.Object
                    )
                )
            );

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider);
            var hostedServices = serviceProvider.GetServices<IHostedService>();
            Assert.Contains(hostedServices, s => s.GetType() == typeof(RabbitMqEventListenerService<Mock>));
        }
    }
}
