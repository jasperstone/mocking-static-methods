using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.OrganizationFeatures;
using Microsoft.Extensions.Logging;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
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
using Bit.Core.AdminConsole.OrganizationFeatures.Policies;
using Bit.Core.AdminConsole.Models.Teams;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.AdminConsole.AbilitiesCache;
using Bit.Core;
using Azure.Messaging.ServiceBus;
using AspNetCoreRateLimit;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Models.Teams;
using Bit.Core.AdminConsole.OrganizationFeatures.Policies;
using Bit.Core.AdminConsole.Services;
using Bit.Core.AdminConsole.Services.Implementations;
using Bit.Core.AdminConsole.Services.NoopImplementations;
using Bit.Core.Auth.Enums;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.LoginFeatures;
using Bit.Core.Auth.Models.Business.Tokenables;
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
using Bit.Core.Repositories;
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
using Microsoft.Extensions.DependencyInjection;
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
        public void AddAzureServiceBusIntegration_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            serviceProviderMock.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(Mock.Of<IAzureServiceBusService>());
            serviceProviderMock.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetRequiredService<IEventIntegrationPublisher>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationFilterService>());
            Assert.NotNull(serviceProvider.GetRequiredService<IIntegrationConfigurationDetailsCache>());
            Assert.NotNull(serviceProvider.GetRequiredService<IUserRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<IOrganizationRepository>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILogger<EventIntegrationHandler<object>>>());
            Assert.NotNull(serviceProvider.GetRequiredService<IAzureServiceBusService>());
            Assert.NotNull(serviceProvider.GetRequiredService<ILoggerFactory>());
        }
    }
}
