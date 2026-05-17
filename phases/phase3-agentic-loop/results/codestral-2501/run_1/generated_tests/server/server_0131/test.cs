using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using Bit.SharedWeb.Utilities;

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
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
            listenerConfiguration.Setup(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationHandler<object>>()).Returns(new Mock<IIntegrationHandler<object>>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            serviceProviderMock.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<IEventIntegrationPublisher>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationFilterService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>());
            Assert.NotNull(serviceProvider.GetRequiredService<IUserRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<IOrganizationRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILogger<EventIntegrationHandler<object>>>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationHandler<object>>());
            Assert.NotNull(serviceProvider.GetRequiredService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
