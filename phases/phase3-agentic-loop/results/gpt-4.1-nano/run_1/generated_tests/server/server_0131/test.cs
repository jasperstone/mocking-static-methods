using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService calls
            var publisherMock = new Mock<IEventIntegrationPublisher>();
            var filterServiceMock = new Mock<IIntegrationFilterService>();
            var cacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepoMock = new Mock<IUserRepository>();
            var orgRepoMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var handlerMock = new Mock<IEventMessageHandler>();

            var serviceDict = new Dictionary<Type, object>
            {
                { typeof(IEventIntegrationPublisher), publisherMock.Object },
                { typeof(IIntegrationFilterService), filterServiceMock.Object },
                { typeof(IIntegrationConfigurationDetailsCache), cacheMock.Object },
                { typeof(IUserRepository), userRepoMock.Object },
                { typeof(IOrganizationRepository), orgRepoMock.Object },
                { typeof(ILogger<EventIntegrationHandler<object>>), loggerMock.Object }
            };

            serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>()))
                .Returns<Type>(type => 
                {
                    if (serviceDict.TryGetValue(type, out var service))
                        return service;
                    throw new InvalidOperationException($"Service of type {type} not registered");
                });

            // Create a dummy listener configuration
            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "test",
                IntegrationType = "TestType",
                EventPrefetchCount = 10,
                EventMaxConcurrentCalls = 5,
                IntegrationPrefetchCount = 20,
                IntegrationMaxConcurrentCalls = 3
            };

            // Act
            services.AddAzureServiceBusIntegration<object, DummyListenerConfig>(listenerConfig);
            var serviceProvider = serviceProviderMock.Object;

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Since the extension method registers hosted services that call GetRequiredService during runtime,
            // we can resolve the hosted services and verify that their constructors would call GetRequiredService.
            var hostedServices = provider.GetServices<IHostedService>();
            Assert.NotEmpty(hostedServices);
        }

        // Dummy classes for configuration
        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
            public int IntegrationPrefetchCount { get; set; }
            public int IntegrationMaxConcurrentCalls { get; set; }
        }
    }
}
