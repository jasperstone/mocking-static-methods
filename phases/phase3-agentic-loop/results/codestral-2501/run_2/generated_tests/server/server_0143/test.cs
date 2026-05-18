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
using System.Collections.Generic;
using System.Linq;
using Azure.Messaging.ServiceBus;
using Bit.Core.AdminConsole.Models.Teams;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
using Bit.Core.HostedServices;
using Bit.Core.Enums;
using Bit.Core.Dirt.Reports.ReportFeatures;
using Bit.Core.Billing.TrialInitiation;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Services;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.Services.Implementations;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Enums;
using Bit.Core.AdminConsole.Services.NoopImplementations;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.OrganizationFeatures.Policies;
using Bit.Core.AdminConsole.Models.Teams;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.AdminConsole.AbilitiesCache;
using Bit.Core;
using AspNetCoreRateLimit;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
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
        public void AddAzureServiceBusIntegration_ShouldAddServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockAzureTableStorageEventHandler = new Mock<AzureTableStorageEventHandler>();
            var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockRepositoryConfiguration = new Mock<IRepositoryConfiguration>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<AzureTableStorageEventHandler>()).Returns(mockAzureTableStorageEventHandler.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IAzureServiceBusService>()).Returns(mockAzureServiceBusService.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IRepositoryConfiguration>()).Returns(mockRepositoryConfiguration.Object);

            var slackConfiguration = new SlackIntegrationConfigurationDetails();
            var webhookConfiguration = new WebhookIntegrationConfigurationDetails();
            var hecConfiguration = new HecListenerConfiguration();
            var datadogConfiguration = new DatadogIntegrationConfigurationDetails();
            var teamsConfiguration = new TeamsIntegrationConfigurationDetails();

            // Act
            serviceCollection.AddAzureServiceBusIntegration<SlackIntegrationConfigurationDetails, SlackListenerConfiguration>(slackConfiguration);
            serviceCollection.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, WebhookListenerConfiguration>(webhookConfiguration);
            serviceCollection.AddAzureServiceBusIntegration<WebhookIntegrationConfigurationDetails, HecListenerConfiguration>(hecConfiguration);
            serviceCollection.AddAzureServiceBusIntegration<DatadogIntegrationConfigurationDetails, DatadogListenerConfiguration>(datadogConfiguration);
            serviceCollection.AddAzureServiceBusIntegration<TeamsIntegrationConfigurationDetails, TeamsListenerConfiguration>(teamsConfiguration);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<AzureTableStorageEventHandler>());
            Assert.NotNull(serviceProvider.GetService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<IRepositoryConfiguration>());
        }
    }
}
