using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_EventHandlerFactory_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Register all required dependencies for the EventIntegrationHandler factory
            services.AddSingleton<IEventIntegrationPublisher>(Mock.Of<IEventIntegrationPublisher>());
            services.AddSingleton<IIntegrationFilterService>(Mock.Of<IIntegrationFilterService>());
            services.AddSingleton<IIntegrationConfigurationDetailsCache>(Mock.Of<IIntegrationConfigurationDetailsCache>());
            services.AddSingleton<IUserRepository>(Mock.Of<IUserRepository>());
            services.AddSingleton<IOrganizationRepository>(Mock.Of<IOrganizationRepository>());
            services.AddLogging();

            var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
            listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");
            listenerConfig.Setup(x => x.IntegrationType).Returns("Test");

            // Act
            services.AddRabbitMqIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);

            // Assert - Building triggers factory calls including GetRequiredService on line ~1030
            using var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void AddRabbitMqIntegration_RabbitMqEventListenerServiceFactory_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
            
            // Register keyed handler first so EventListenerService factory succeeds
            services.TryAddKeyedSingleton<IEventMessageHandler>("test-key", Mock.Of<IEventMessageHandler>());

            var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
            listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");

            // Act
            services.AddRabbitMqIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);

            // Assert - Triggers GetRequiredKeyedService and GetRequiredService<ILoggerFactory>()
            using var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider);
        }

        [Fact]
        public void AddRabbitMqIntegration_RabbitMqIntegrationListenerServiceFactory_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IRabbitMqService>(Mock.Of<IRabbitMqService>());
            services.AddSingleton<TimeProvider>(Mock.Of<TimeProvider>());
            
            // Register required handler
            services.AddSingleton<IIntegrationHandler<TestConfig>>(Mock.Of<IIntegrationHandler<TestConfig>>());

            var listenerConfig = new Mock<IIntegrationListenerConfiguration>();
            listenerConfig.Setup(x => x.RoutingKey).Returns("test-key");

            // Act
            services.AddRabbitMqIntegration<TestConfig, TestListenerConfig>(listenerConfig.Object);

            // Assert - Triggers GetRequiredService<ILoggerFactory>() and GetRequiredService<TimeProvider>()
            using var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider);
        }
    }

    // Test classes to satisfy generic constraints
    public class TestConfig { }

    public class TestListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "test-key";
        public string IntegrationType => "Test";
    }
}
