using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldAddServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockAzureTableStorageEventHandler = new Mock<AzureTableStorageEventHandler>();
            var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockRepositoryConfiguration = new Mock<IRepositoryConfiguration>();

            mockProvider.Setup(x => x.GetRequiredService<AzureTableStorageEventHandler>()).Returns(mockAzureTableStorageEventHandler.Object);
            mockProvider.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(mockAzureServiceBusService.Object);
            mockProvider.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockProvider.Setup(x => x.GetRequiredService<IRepositoryConfiguration>()).Returns(mockRepositoryConfiguration.Object);

            // Act
            services.AddAzureServiceBusIntegration<SlackIntegrationConfigurationDetails, SlackListenerConfiguration>(new SlackListenerConfiguration());
            services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(new WebhookListenerConfiguration());
            services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(new HecListenerConfiguration());
            services.AddAzureServiceBusIntegration<DatadogIntegrationConfigurationDetails, DatadogListenerConfiguration>(new DatadogListenerConfiguration());
            services.AddAzureServiceBusIntegration<TeamsIntegrationConfigurationDetails, TeamsListenerConfiguration>(new TeamsListenerConfiguration());

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<AzureTableStorageEventHandler>());
            Assert.NotNull(serviceProvider.GetService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<IRepositoryConfiguration>());
        }
    }
}
