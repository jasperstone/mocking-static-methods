using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfigurationMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigurationMock.SetupGet(l => l.RoutingKey).Returns("test-routing-key");
            listenerConfigurationMock.SetupGet(l => l.IntegrationType).Returns("test-integration-type");
            listenerConfigurationMock.SetupGet(l => l.EventPrefetchCount).Returns(10);
            listenerConfigurationMock.SetupGet(l => l.EventMaxConcurrentCalls).Returns(5);
            listenerConfigurationMock.SetupGet(l => l.IntegrationPrefetchCount).Returns(10);
            listenerConfigurationMock.SetupGet(l => l.IntegrationMaxConcurrentCalls).Returns(5);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(eventIntegrationPublisherMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(integrationFilterServiceMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(configurationCacheMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(userRepositoryMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(organizationRepositoryMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(loggerMock.Object);

            // Act
            ServiceCollectionExtensions
                .AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(services, listenerConfigurationMock.Object);

            // Assert
            var serviceDescriptors = services
                .Where(sd => sd.ServiceType == typeof(IEventMessageHandler))
                .ToList();

            Assert.Single(serviceDescriptors);

            var keyedSingletonDescriptor = serviceDescriptors.First();
            var implementationFactory = (Func<IServiceProvider, object, object>)keyedSingletonDescriptor.ImplementationFactory;
            var handler = (EventIntegrationHandler<object>)implementationFactory(serviceProviderMock.Object, null);

            Assert.Same(eventIntegrationPublisherMock.Object, handler.EventIntegrationPublisher);
            Assert.Same(integrationFilterServiceMock.Object, handler.IntegrationFilterService);
            Assert.Same(configurationCacheMock.Object, handler.ConfigurationCache);
            Assert.Same(userRepositoryMock.Object, handler.UserRepository);
            Assert.Same(organizationRepositoryMock.Object, handler.OrganizationRepository);
            Assert.Same(loggerMock.Object, handler.Logger);

            var hostedServiceDescriptors = services
                .Where(sd => sd.ServiceType == typeof(IHostedService))
                .ToList();

            Assert.Equal(2, hostedServiceDescriptors.Count);

            var azureServiceBusEventListenerServiceDescriptor = hostedServiceDescriptors
                .First(sd => sd.ImplementationType == typeof(AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>));

            var azureServiceBusEventListenerServiceFactory = (Func<IServiceProvider, IHostedService>)azureServiceBusEventListenerServiceDescriptor.ImplementationFactory;
            var azureServiceBusEventListenerService = (AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>)azureServiceBusEventListenerServiceFactory(serviceProviderMock.Object);

            Assert.Same(listenerConfigurationMock.Object, azureServiceBusEventListenerService.Configuration);
            Assert.Same(handler, azureServiceBusEventListenerService.Handler);
            Assert.NotNull(azureServiceBusEventListenerService.ServiceBusService);
            Assert.NotNull(azureServiceBusEventListenerService.ServiceBusOptions);
            Assert.NotNull(azureServiceBusEventListenerService.LoggerFactory);

            var azureServiceBusIntegrationListenerServiceDescriptor = hostedServiceDescriptors
                .First(sd => sd.ImplementationType == typeof(AzureServiceBusIntegrationListenerService<IIntegrationListenerConfiguration>));

            var azureServiceBusIntegrationListenerServiceFactory = (Func<IServiceProvider, IHostedService>)azureServiceBusIntegrationListenerServiceDescriptor.ImplementationFactory;
            var azureServiceBusIntegrationListenerService = (AzureServiceBusIntegrationListenerService<IIntegrationListenerConfiguration>)azureServiceBusIntegrationListenerServiceFactory(serviceProviderMock.Object);

            Assert.Same(listenerConfigurationMock.Object, azureServiceBusIntegrationListenerService.Configuration);
            Assert.NotNull(azureServiceBusIntegrationListenerService.Handler);
            Assert.NotNull(azureServiceBusIntegrationListenerService.ServiceBusService);
            Assert.NotNull(azureServiceBusIntegrationListenerService.ServiceBusOptions);
        }
    }
}
