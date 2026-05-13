using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Entities;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Vault;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testType");
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(1);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(1);
            listenerConfiguration.Setup(x => x.IntegrationPrefetchCount).Returns(1);
            listenerConfiguration.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(1);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredKeyedService<IEventMessageHandler>("testKey")).Returns(new Mock<IEventMessageHandler>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationHandler<object>>()).Returns(new Mock<IIntegrationHandler<object>>().Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService);
        }
    }
}
