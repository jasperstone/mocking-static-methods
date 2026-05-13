using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Bit.Core;
using Bit.Core.Auth;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Models.Business;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.Business;
using Bit.Core.Models.Data;
using Bit.Core.Models.Request;
using Bit.Core.Models.Response;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Tools;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddAzureServiceBusIntegration_ValidConfig_AddsServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns("routingKey");
            listenerConfiguration.SetupGet(x => x.IntegrationType).Returns(IntegrationType.Event);
            listenerConfiguration.SetupGet(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.SetupGet(x => x.EventMaxConcurrentCalls).Returns(5);

            // Act
            services.AddAzureServiceBusIntegration(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            var azureServiceBusEventListenerService = serviceProvider.GetService<AzureServiceBusEventListenerService<IIntegrationListenerConfiguration>>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(azureServiceBusEventListenerService);
        }

        [Fact]
        public async Task AddAzureServiceBusIntegration_InvalidConfig_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(x => x.RoutingKey).Returns(null);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => services.AddAzureServiceBusIntegration(listenerConfiguration.Object));
        }
    }
}
