using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.HostedServices;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListener_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var globalSettings = new Mock<GlobalSettings>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<IntegrationConfig>>>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IRabbitMqService>()).Returns(Mock.Of<IRabbitMqService>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>()).Returns(Mock.Of<TimeProvider>());

            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddEventIntegrationListener<IntegrationConfig, IntegrationListenerConfig>(listenerConfiguration.Object, globalSettings.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventIntegrationPublisher = serviceProvider.GetRequiredService<IEventIntegrationPublisher>();
            var integrationFilterService = serviceProvider.GetRequiredService<IIntegrationFilterService>();
            var integrationConfigurationDetailsCache = serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>();
            var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
            var organizationRepository = serviceProvider.GetRequiredService<IOrganizationRepository>();
            var logger = serviceProvider.GetRequiredService<ILogger<EventIntegrationHandler<IntegrationConfig>>>();
            var rabbitMqService = serviceProvider.GetRequiredService<IRabbitMqService>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            Assert.NotNull(eventIntegrationPublisher);
            Assert.NotNull(integrationFilterService);
            Assert.NotNull(integrationConfigurationDetailsCache);
            Assert.NotNull(userRepository);
            Assert.NotNull(organizationRepository);
            Assert.NotNull(logger);
            Assert.NotNull(rabbitMqService);
            Assert.NotNull(loggerFactory);
            Assert.NotNull(timeProvider);
        }
    }
}
