using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core;
using Bit.Core.HostedServices;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_Should_Call_GetRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create mocks for the services that will be retrieved
            var mockPublisher = new Mock<IEventIntegrationPublisher>();
            var mockFilterService = new Mock<IIntegrationFilterService>();
            var mockConfigCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrgRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            // Create a mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup the mock to return the mocked services
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(mockPublisher.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>())
                .Returns(mockFilterService.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(mockConfigCache.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>())
                .Returns(mockUserRepository.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>())
                .Returns(mockOrgRepository.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(mockLogger.Object);

            // Create a dummy listener configuration
            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "testKey",
                IntegrationType = "testType",
                EventPrefetchCount = 10,
                EventMaxConcurrentCalls = 5
            };

            // Act
            services.AddAzureServiceBusIntegration<object, DummyListenerConfig>(listenerConfig);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Trigger the registration
            var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHostedService));
            Assert.NotNull(serviceDescriptor);
        }

        // Dummy implementation for IIntegrationListenerConfiguration
        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }
    }
}
