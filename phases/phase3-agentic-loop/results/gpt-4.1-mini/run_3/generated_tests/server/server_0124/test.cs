using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.NotificationCenter;
using Bit.Core.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Platform;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
        }

        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }

        private class TestConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesAndResolvesDependencies()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new TestListenerConfig
            {
                RoutingKey = "testRoutingKey",
                IntegrationType = "testIntegrationType",
                EventPrefetchCount = 5,
                EventMaxConcurrentCalls = 10
            };

            // Register mocks for all required services
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<Bit.Core.NotificationCenter.EventIntegrationHandler<TestConfig>>>();
            var azureServiceBusServiceMock = new Mock<IAzureServiceBusService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            services.AddSingleton(eventIntegrationPublisherMock.Object);
            services.AddSingleton(integrationFilterServiceMock.Object);
            services.AddSingleton(configurationCacheMock.Object);
            services.AddSingleton(userRepositoryMock.Object);
            services.AddSingleton(organizationRepositoryMock.Object);
            services.AddSingleton(loggerMock.Object);
            services.AddSingleton(azureServiceBusServiceMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);

            // Use reflection to invoke the private extension method AddAzureServiceBusIntegration<TConfig, TListenerConfig>
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(TestConfig), typeof(TestListenerConfig));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            var serviceProvider = services.BuildServiceProvider();

            Assert.Same(eventIntegrationPublisherMock.Object, serviceProvider.GetRequiredService<IEventIntegrationPublisher>());
            Assert.Same(integrationFilterServiceMock.Object, serviceProvider.GetRequiredService<IIntegrationFilterService>());
            Assert.Same(configurationCacheMock.Object, serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>());
            Assert.Same(userRepositoryMock.Object, serviceProvider.GetRequiredService<IUserRepository>());
            Assert.Same(organizationRepositoryMock.Object, serviceProvider.GetRequiredService<IOrganizationRepository>());
            Assert.Same(loggerMock.Object, serviceProvider.GetRequiredService<ILogger<Bit.Core.NotificationCenter.EventIntegrationHandler<TestConfig>>>());
            Assert.Same(azureServiceBusServiceMock.Object, serviceProvider.GetRequiredService<IAzureServiceBusService>());
            Assert.Same(loggerFactoryMock.Object, serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
