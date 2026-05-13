using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testType");
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(5);

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

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<IEventMessageHandler>());
            Assert.NotNull(serviceProvider.GetRequiredService<IHostedService>());
        }
    }
}
