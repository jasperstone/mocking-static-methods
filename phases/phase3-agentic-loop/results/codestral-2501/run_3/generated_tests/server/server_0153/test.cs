using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationServices_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var globalSettings = new Mock<GlobalSettings>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IEventIntegrationPublisher>())
                .Returns(Mock.Of<IEventIntegrationPublisher>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IIntegrationFilterService>())
                .Returns(Mock.Of<IIntegrationFilterService>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IUserRepository>())
                .Returns(Mock.Of<IUserRepository>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IOrganizationRepository>())
                .Returns(Mock.Of<IOrganizationRepository>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>())
                .Returns(Mock.Of<ILogger<EventIntegrationHandler<IntegrationConfig>>>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IRabbitMqService>())
                .Returns(Mock.Of<IRabbitMqService>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());

            serviceProviderMock
                .Setup(x => x.GetRequiredService<TimeProvider>())
                .Returns(Mock.Of<TimeProvider>());

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddEventIntegrationServices<IntegrationConfig>(listenerConfiguration.Object, globalSettings.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<IEventIntegrationPublisher>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationFilterService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>());
            Assert.NotNull(serviceProvider.GetRequiredService<IUserRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<IOrganizationRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>());
            Assert.NotNull(serviceProvider.GetRequiredService<IRabbitMqService>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetRequiredService<TimeProvider>());
        }
    }
}
