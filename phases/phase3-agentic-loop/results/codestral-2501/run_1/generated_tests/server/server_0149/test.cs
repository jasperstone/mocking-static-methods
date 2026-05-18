using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Entities;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(c => c.RoutingKey).Returns("testRoutingKey");
            listenerConfiguration.Setup(c => c.IntegrationType).Returns("testIntegrationType");

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<HecListenerConfiguration>>>();
            var rabbitMqService = new Mock<IRabbitMqService>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var timeProvider = new Mock<TimeProvider>();

            serviceCollection.AddSingleton(eventIntegrationPublisher.Object);
            serviceCollection.AddSingleton(integrationFilterService.Object);
            serviceCollection.AddSingleton(configurationCache.Object);
            serviceCollection.AddSingleton(userRepository.Object);
            serviceCollection.AddSingleton(organizationRepository.Object);
            serviceCollection.AddSingleton(logger.Object);
            serviceCollection.AddSingleton(rabbitMqService.Object);
            serviceCollection.AddSingleton(loggerFactory.Object);
            serviceCollection.AddSingleton(timeProvider.Object);

            // Act
            serviceCollection.AddRabbitMqIntegration<HecListenerConfiguration, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService1 = serviceProvider.GetRequiredService<IHostedService>();
            var hostedService2 = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService1);
            Assert.NotNull(hostedService2);
        }
    }
}
