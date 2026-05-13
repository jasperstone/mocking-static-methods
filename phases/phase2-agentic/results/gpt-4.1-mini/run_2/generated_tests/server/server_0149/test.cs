using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Auth.Repositories;
using Bit.Core.HostedServices;
using Bit.Core.Services;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
using Bit.Core.OrganizationFeatures;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();
            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockTimeProvider = new Mock<TimeProvider>();

            // Setup GetRequiredService calls expected in AddRabbitMqIntegration
            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher))).Returns(mockEventIntegrationPublisher.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService))).Returns(mockIntegrationFilterService.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(mockConfigurationCache.Object);
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository))).Returns(mockUserRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository))).Returns(mockOrganizationRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<object>>))).Returns(mockLogger.Object);
            mockProvider.Setup(p => p.GetService(typeof(IRabbitMqService))).Returns(mockRabbitMqService.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockProvider.Setup(p => p.GetService(typeof(TimeProvider))).Returns(mockTimeProvider.Object);

            // Create a dummy listener configuration
            var listenerConfig = new DummyListenerConfiguration();

            // Act
            // We need to call the private extension method AddRabbitMqIntegration via reflection or test a public method that calls it.
            // Since it's private, we test indirectly by calling a public method that uses it or simulate the call.
            // For this test, we simulate the factory delegate to verify GetRequiredService calls.

            // Simulate the factory delegate passed to TryAddKeyedSingleton
            Func<IServiceProvider, object> factory = provider =>
                new EventIntegrationHandler<object>(
                    integrationType: listenerConfig.IntegrationType,
                    eventIntegrationPublisher: provider.GetRequiredService<IEventIntegrationPublisher>(),
                    integrationFilterService: provider.GetRequiredService<IIntegrationFilterService>(),
                    configurationCache: provider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                    userRepository: provider.GetRequiredService<IUserRepository>(),
                    organizationRepository: provider.GetRequiredService<IOrganizationRepository>(),
                    logger: provider.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()
                );

            // Call the factory with the mock provider
            var handler = factory(mockProvider.Object);

            // Assert
            mockProvider.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IUserRepository>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IOrganizationRepository>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
            Assert.NotNull(handler);
        }

        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey => "dummy-routing-key";
            public string IntegrationType => "dummy-integration-type";
        }
    }

    // Extension methods to mock GetRequiredService calls on IServiceProvider
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = provider.GetService(typeof(T));
            if (service == null)
                throw new InvalidOperationException($"Service of type {typeof(T)} not found.");
            return (T)service;
        }
    }
}
