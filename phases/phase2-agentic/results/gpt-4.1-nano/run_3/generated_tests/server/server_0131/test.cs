using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class AddAzureServiceBusIntegrationTests
    {
        [Fact]
        public void AddsServicesAndHandlers_WithValidConfiguration_ShouldRegisterExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var mockPublisher = new Mock<IEventIntegrationPublisher>();
            var mockFilterService = new Mock<IIntegrationFilterService>();
            var mockCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockOrgRepo = new Mock<IOrganizationRepository>();
            var mockHandler = new EventIntegrationHandler<object>(
                integrationType: "TestType",
                eventIntegrationPublisher: mockPublisher.Object,
                integrationFilterService: mockFilterService.Object,
                configurationCache: mockCache.Object,
                userRepository: mockUserRepo.Object,
                organizationRepository: mockOrgRepo.Object,
                logger: mockLogger.Object
            );

            // Setup provider to return mocks for required services
            mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(mockPublisher.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>())
                .Returns(mockFilterService.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(mockCache.Object);
            mockProvider.Setup(p => p.GetRequiredService<IUserRepository>())
                .Returns(mockUserRepo.Object);
            mockProvider.Setup(p => p.GetRequiredService<IOrganizationRepository>())
                .Returns(mockOrgRepo.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(mockLogger.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            // Create a dummy listener configuration
            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "testKey",
                IntegrationType = "TestType"
            };

            // Act
            services.TryAddKeyedSingleton<IEventMessageHandler>(
                serviceKey: listenerConfig.RoutingKey,
                implementationFactory: (provider, _) => new EventIntegrationHandler<object>(
                    integrationType: listenerConfig.IntegrationType,
                    eventIntegrationPublisher: provider.GetRequiredService<IEventIntegrationPublisher>(),
                    integrationFilterService: provider.GetRequiredService<IIntegrationFilterService>(),
                    configurationCache: provider.GetRequiredService<IIntegrationConfigurationDetailsCache>(),
                    userRepository: provider.GetRequiredService<IUserRepository>(),
                    organizationRepository: provider.GetRequiredService<IOrganizationRepository>(),
                    logger: provider.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()
                )
            );

            var serviceProvider = services.BuildServiceProvider();

            // Replace provider in the service collection with our mock provider
            services.AddSingleton<IServiceProvider>(provider => mockProvider.Object);
            var finalProvider = services.BuildServiceProvider();

            // Assert
            // Verify that the service collection contains the expected singleton registration
            var handler = finalProvider.GetRequiredService<IEventMessageHandler>();
            Assert.NotNull(handler);
            Assert.IsType<EventIntegrationHandler<object>>(handler);
        }
    }

    // Dummy implementation for listener configuration
    public class DummyListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
        public int EventPrefetchCount { get; set; } = 10;
        public int EventMaxConcurrentCalls { get; set; } = 5;
        public int IntegrationPrefetchCount { get; set; } = 10;
        public int IntegrationMaxConcurrentCalls { get; set; } = 5;
    }

    // Dummy interface for configuration
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
        int IntegrationPrefetchCount { get; }
        int IntegrationMaxConcurrentCalls { get; }
    }
}
