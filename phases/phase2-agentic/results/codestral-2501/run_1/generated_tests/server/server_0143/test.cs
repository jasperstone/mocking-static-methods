using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;
using Bit.Core.Tools.Services;
using Bit.Core.Vault.Services;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.Push;
using Bit.Core.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Repositories.Noop;
using Bit.Core.Tokens;
using Bit.Core.Tools.ImportFeatures;
using Bit.Core.Tools.SendFeatures;
using Bit.Core.Utilities;
using Bit.Core.Vault;
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
        public void AddAzureServiceBusIntegration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetRequiredService<AzureTableStorageEventHandler>()).Returns(new AzureTableStorageEventHandler());
            provider.Setup(p => p.GetRequiredService<IAzureServiceBusService>()).Returns(new Mock<IAzureServiceBusService>().Object);
            provider.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(new Mock<ILoggerFactory>().Object);

            var repositoryConfiguration = new Mock<IRepositoryConfiguration>();
            repositoryConfiguration.Setup(r => r.EventPrefetchCount).Returns(10);
            repositoryConfiguration.Setup(r => r.EventMaxConcurrentCalls).Returns(5);

            var slackConfiguration = new Mock<SlackIntegrationConfigurationDetails>();
            var webhookConfiguration = new Mock<WebhookIntegrationConfigurationDetails>();
            var hecConfiguration = new Mock<HecListenerConfiguration>();
            var datadogConfiguration = new Mock<DatadogIntegrationConfigurationDetails>();
            var teamsConfiguration = new Mock<TeamsIntegrationConfigurationDetails>();

            // Act
            services.AddAzureServiceBusIntegration<SlackIntegrationConfigurationDetails, SlackListenerConfiguration>(slackConfiguration.Object);
            services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(webhookConfiguration.Object);
            services.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(hecConfiguration.Object);
            services.AddAzureServiceBusIntegration<DatadogIntegrationConfigurationDetails, DatadogListenerConfiguration>(datadogConfiguration.Object);
            services.AddAzureServiceBusIntegration<TeamsIntegrationConfigurationDetails, TeamsListenerConfiguration>(teamsConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<AzureTableStorageEventHandler>());
            Assert.NotNull(serviceProvider.GetService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
        }

        [Fact]
        public void AddRabbitMqIntegration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = new Mock<IServiceProvider>();
            provider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(new Mock<IEventIntegrationPublisher>().Object);
            provider.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(new Mock<IIntegrationFilterService>().Object);
            provider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);
            provider.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(new Mock<IUserRepository>().Object);
            provider.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(new Mock<IOrganizationRepository>().Object);
            provider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<SlackIntegrationConfigurationDetails>>>()).Returns(new Mock<ILogger<EventIntegrationHandler<SlackIntegrationConfigurationDetails>>>().Object);
            provider.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(new Mock<IRabbitMqService>().Object);
            provider.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(new Mock<TimeProvider>().Object);

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.Setup(l => l.RoutingKey).Returns("testRoutingKey");
            listenerConfiguration.Setup(l => l.IntegrationType).Returns("testIntegrationType");

            // Act
            services.AddRabbitMqIntegration<SlackIntegrationConfigurationDetails, IIntegrationListenerConfiguration>(listenerConfiguration.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IEventIntegrationPublisher>());
            Assert.NotNull(serviceProvider.GetService<IIntegrationFilterService>());
            Assert.NotNull(serviceProvider.GetService<IIntegrationConfigurationDetailsCache>());
            Assert.NotNull(serviceProvider.GetService<IUserRepository>());
            Assert.NotNull(serviceProvider.GetService<IOrganizationRepository>());
            Assert.NotNull(serviceProvider.GetService<ILogger<EventIntegrationHandler<SlackIntegrationConfigurationDetails>>>());
            Assert.NotNull(serviceProvider.GetService<IRabbitMqService>());
            Assert.NotNull(serviceProvider.GetService<TimeProvider>());
        }
    }
}
