using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.Extensions.Logging;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Services.Implementations;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
using Bit.Core.Billing.Services;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.TrialInitiation;
using Bit.Core.Dirt.Reports.ReportFeatures;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.HostedServices;
using Bit.Core.KeyManagement;
using Bit.Core.NotificationCenter;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Resources;
using Bit.Core.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Repositories.Noop;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Services.Mail;
using Bit.Core.Settings;
using Bit.Core.Tokens;
using Bit.Core.Tools.ImportFeatures;
using Bit.Core.Tools.SendFeatures;
using Bit.Core.Tools.Services;
using Bit.Core.Utilities;
using Bit.Core.Vault;
using Bit.Core.Vault.Services;
using Bit.Infrastructure.Dapper;
using Bit.Infrastructure.EntityFramework;
using DnsClient;
using Duende.IdentityModel;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using NoopRepos = Bit.Core.Repositories.Noop;
using Role = Bit.Core.Entities.Role;
using TableStorageRepos = Bit.Core.Repositories.TableStorage;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterEventMessageHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.IntegrationType).Returns("testType");

            var eventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var integrationFilterService = new Mock<IIntegrationFilterService>();
            var configurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepository = new Mock<IUserRepository>();
            var organizationRepository = new Mock<IOrganizationRepository>();
            var logger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            services.AddSingleton(eventIntegrationPublisher.Object);
            services.AddSingleton(integrationFilterService.Object);
            services.AddSingleton(configurationCache.Object);
            services.AddSingleton(userRepository.Object);
            services.AddSingleton(organizationRepository.Object);
            services.AddSingleton(logger.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var handler = serviceProvider.GetRequiredKeyedService<IEventMessageHandler>("testKey");
            Assert.NotNull(handler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterAzureServiceBusEventListenerService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.RoutingKey).Returns("testKey");
            listenerConfiguration.Setup(x => x.EventPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.EventMaxConcurrentCalls).Returns(5);

            var eventMessageHandler = new Mock<IEventMessageHandler>();
            var azureServiceBusService = new Mock<IAzureServiceBusService>();
            var loggerFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(eventMessageHandler.Object);
            services.AddSingleton(azureServiceBusService.Object);
            services.AddSingleton(loggerFactory.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.NotNull(hostedService);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldRegisterAzureServiceBusIntegrationListenerService()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(x => x.IntegrationPrefetchCount).Returns(10);
            listenerConfiguration.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            var integrationHandler = new Mock<IIntegrationHandler<object>>();
            var azureServiceBusService = new Mock<IAzureServiceBusService>();
            var loggerFactory = new Mock<ILoggerFactory>();

            services.AddSingleton(integrationHandler.Object);
            services.AddSingleton(azureServiceBusService.Object);
            services.AddSingleton(loggerFactory.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();
            Assert.NotNull(hostedService);
        }
    }
}
