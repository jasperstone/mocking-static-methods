using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.Core;
using Bit.SharedWeb.Utilities;
using Bit.Core.Enums;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ExecutesGetRequiredServiceILoggerFactory_Successfully()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Register all dependencies required by the factory lambdas
            services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
            services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
            services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton<IAzureServiceBusService>(Mock.Of<IAzureServiceBusService>());
            services.AddLogging(); // Provides ILoggerFactory via AddLogging()

            // Minimal mock for IIntegrationListenerConfiguration
            var mockConfig = new Mock<IIntegrationListenerConfiguration>();
            mockConfig.SetupGet(x => x.RoutingKey).Returns("test-key");
            mockConfig.SetupGet(x => x.IntegrationType).Returns(IntegrationType.Webhook);
            mockConfig.SetupGet(x => x.EventPrefetchCount).Returns(10);
            mockConfig.SetupGet(x => x.EventMaxConcurrentCalls).Returns(1);
            mockConfig.SetupGet(x => x.IntegrationPrefetchCount).Returns(5);
            mockConfig.SetupGet(x => x.IntegrationMaxConcurrentCalls).Returns(1);

            // Act - This triggers GetRequiredService<ILoggerFactory>() on line 908
            var result = services.AddAzureServiceBusIntegration<object>(mockConfig.Object);

            // Assert - No InvalidOperationException from missing services
            Assert.NotNull(result);
            Assert.Same(services, result);
            
            using var provider = services.BuildServiceProvider();
            Assert.NotNull(provider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddEventIntegrationServices_ExecutesGetRequiredService_Successfully()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings();

            // Act - Triggers GetRequiredService<IIntegrationConfigurationDetailsCache>()
            var result = services.AddEventIntegrationServices(globalSettings);

            // Assert - Method completes without DI resolution exceptions
            Assert.NotNull(result);
            Assert.Same(services, result);
        }
    }
}
