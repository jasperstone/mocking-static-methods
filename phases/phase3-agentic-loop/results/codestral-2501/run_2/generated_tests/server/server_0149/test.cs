using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
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
        public void AddRabbitMqIntegration_ShouldAddServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(c => c.RoutingKey).Returns("testRoutingKey");
            listenerConfiguration.Setup(c => c.IntegrationType).Returns("testIntegrationType");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<TestConfig>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<TestConfig>>>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationHandler<TestConfig>>()).Returns(new Mock<IIntegrationHandler<TestConfig>>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IRabbitMqService>()).Returns(new Mock<IRabbitMqService>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<TimeProvider>()).Returns(new Mock<TimeProvider>().Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddRabbitMqIntegration<TestConfig, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService1 = serviceProvider.GetRequiredService<IHostedService>();
            var hostedService2 = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService1);
            Assert.NotNull(hostedService2);
        }

        private class TestConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }
    }
}
