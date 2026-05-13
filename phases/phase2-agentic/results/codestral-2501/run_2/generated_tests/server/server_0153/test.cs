using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Microsoft.Extensions.Logging;
using Bit.Core.Utilities;
using Bit.Core.HostedServices;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListener_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var globalSettings = new Mock<GlobalSettings>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<IntegrationConfig>>>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IRabbitMqService>()).Returns(Mock.Of<IRabbitMqService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(x => x.GetRequiredService<TimeProvider>()).Returns(Mock.Of<TimeProvider>());

            var serviceProvider = serviceProviderMock.Object;

            // Act
            services.AddEventIntegrationListener<IntegrationConfig, IntegrationListenerConfig>(listenerConfiguration.Object, globalSettings.Object);

            // Assert
            var serviceProviderIsBuilt = services.BuildServiceProvider();
            Assert.NotNull(serviceProviderIsBuilt.GetService<IEventMessageHandler>());
            Assert.NotNull(serviceProviderIsBuilt.GetService<IHostedService>());
        }
    }
}
