using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();
            
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(x => x.RoutingKey).Returns("test.key");
            mockListenerConfig.Setup(x => x.IntegrationType).Returns("Test");

            // Register minimal mocks for all required dependencies
            services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
            services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
            services.AddSingleton<ILogger<EventIntegrationHandler<object>>>(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
            services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
            services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());

            // Act
            var result = services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfig.Object);

            // Assert
            Assert.Same(services, result);
            
            // Build provider to trigger factory lambdas including GetRequiredService calls (line 1030)
            var provider = services.BuildServiceProvider();
            
            // Verify factories executed without exception (covers GetRequiredService calls)
            var hostedServices = provider.GetServices<IHostedService>();
            Assert.NotEmpty(hostedServices);
        }
    }
}
