using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.Extensions.Logging;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Azure.Messaging.ServiceBus;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldAddServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
            var mockRepositoryConfiguration = new Mock<IRepositoryConfiguration>();

            mockServiceProvider
                .Setup(x => x.GetRequiredService<AzureTableStorageEventHandler>())
                .Returns(new AzureTableStorageEventHandler());

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IAzureServiceBusService>())
                .Returns(mockAzureServiceBusService.Object);

            mockServiceProvider
                .Setup(x => x.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IRepositoryConfiguration>())
                .Returns(mockRepositoryConfiguration.Object);

            var slackConfiguration = new SlackIntegrationConfigurationDetails();

            // Act
            serviceCollection.AddAzureServiceBusIntegration<SlackIntegrationConfigurationDetails, SlackListenerConfiguration>(slackConfiguration);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<AzureTableStorageEventHandler>());
            Assert.NotNull(serviceProvider.GetService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<IRepositoryConfiguration>());
        }
    }
}
