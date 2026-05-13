using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddIntegrationListener_Should_Add_Required_Services()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var listenerConfigurationMock = new Mock<IIntegrationListenerConfiguration>();
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var rabbitMqServiceMock = new Mock<IRabbitMqService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var timeProviderMock = new Mock<TimeProvider>();

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

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IRabbitMqService>())
                .Returns(rabbitMqServiceMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<TimeProvider>())
                .Returns(timeProviderMock.Object);

            listenerConfigurationMock
                .SetupGet(c => c.RoutingKey)
                .Returns("test-routing-key");

            listenerConfigurationMock
                .SetupGet(c => c.IntegrationType)
                .Returns("test-integration-type");

            // Act
            ServiceCollectionExtensions
                .AddIntegrationListener(services, listenerConfigurationMock.Object, serviceProviderMock.Object);

            // Assert
            var serviceDescriptors = services
                .Where(sd => sd.ServiceType == typeof(IEventMessageHandler))
                .ToList();

            Assert.Single(serviceDescriptors);

            var eventMessageHandlerDescriptor = serviceDescriptors.First();
            var eventMessageHandlerFactory = (Func<IServiceProvider, object, object>)eventMessageHandlerDescriptor.ImplementationFactory;

            var eventIntegrationHandler = (EventIntegrationHandler<object>)eventMessageHandlerFactory(serviceProviderMock.Object, null);

            Assert.Same(eventIntegrationPublisherMock.Object, eventIntegrationHandler.EventIntegrationPublisher);
            Assert.Same(integrationFilterServiceMock.Object, eventIntegrationHandler.IntegrationFilterService);
            Assert.Same(configurationCacheMock.Object, eventIntegrationHandler.ConfigurationCache);
            Assert.Same(userRepositoryMock.Object, eventIntegrationHandler.UserRepository);
            Assert.Same(organizationRepositoryMock.Object, eventIntegrationHandler.OrganizationRepository);
            Assert.Same(loggerMock.Object, eventIntegrationHandler.Logger);

            var hostedServiceDescriptors = services
                .Where(sd => sd.ServiceType == typeof(IHostedService))
                .ToList();

            Assert.Equal(2, hostedServiceDescriptors.Count);

            var rabbitMqEventListenerServiceDescriptor = hostedServiceDescriptors
                .First(sd => sd.ImplementationType == typeof(RabbitMqEventListenerService<object>));

            var rabbitMqEventListenerServiceFactory = (Func<IServiceProvider, object>)rabbitMqEventListenerServiceDescriptor.ImplementationFactory;

            var rabbitMqEventListenerService = (RabbitMqEventListenerService<object>)rabbitMqEventListenerServiceFactory(serviceProviderMock.Object);

            Assert.Same(eventIntegrationHandler, rabbitMqEventListenerService.Handler);
            Assert.Same(listenerConfigurationMock.Object, rabbitMqEventListenerService.Configuration);
            Assert.Same(rabbitMqServiceMock.Object, rabbitMqEventListenerService.RabbitMqService);
            Assert.Same(loggerFactoryMock.Object, rabbitMqEventListenerService.LoggerFactory);

            var rabbitMqIntegrationListenerServiceDescriptor = hostedServiceDescriptors
                .First(sd => sd.ImplementationType == typeof(RabbitMqIntegrationListenerService<object>));

            var rabbitMqIntegrationListenerServiceFactory = (Func<IServiceProvider, object>)rabbitMqIntegrationListenerServiceDescriptor.ImplementationFactory;

            var rabbitMqIntegrationListenerService = (RabbitMqIntegrationListenerService<object>)rabbitMqIntegrationListenerServiceFactory(serviceProviderMock.Object);

            Assert.Same(eventIntegrationHandler, rabbitMqIntegrationListenerService.Handler);
            Assert.Same(listenerConfigurationMock.Object, rabbitMqIntegrationListenerService.Configuration);
            Assert.Same(rabbitMqServiceMock.Object, rabbitMqIntegrationListenerService.RabbitMqService);
            Assert.Same(loggerFactoryMock.Object, rabbitMqIntegrationListenerService.LoggerFactory);
            Assert.Same(timeProviderMock.Object, rabbitMqIntegrationListenerService.TimeProvider);
        }
    }
}
