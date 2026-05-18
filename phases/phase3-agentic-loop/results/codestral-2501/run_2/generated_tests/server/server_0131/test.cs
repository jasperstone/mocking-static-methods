using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Logging;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Billing.Services;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        public void AddAzureServiceBusIntegration_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockListenerConfig = new Mock<IIntegrationListenerConfiguration>();
            mockListenerConfig.Setup(x => x.RoutingKey).Returns("testRoutingKey");
            mockListenerConfig.Setup(x => x.IntegrationType).Returns("testIntegrationType");
            mockListenerConfig.Setup(x => x.EventPrefetchCount).Returns(10);
            mockListenerConfig.Setup(x => x.EventMaxConcurrentCalls).Returns(5);
            mockListenerConfig.Setup(x => x.IntegrationPrefetchCount).Returns(10);
            mockListenerConfig.Setup(x => x.IntegrationMaxConcurrentCalls).Returns(5);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            mockServiceProvider.Setup(x => x.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            mockServiceProvider.Setup(x => x.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            mockServiceProvider.Setup(x => x.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            mockServiceProvider.Setup(x => x.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            mockServiceProvider.Setup(x => x.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<object>>>());
            mockServiceProvider.Setup(x => x.GetRequiredService<IAzureServiceBusService>()).Returns(Mock.Of<IAzureServiceBusService>());
            mockServiceProvider.Setup(x => x.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());

            services.AddSingleton(mockServiceProvider.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(mockListenerConfig.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService1 = serviceProvider.GetRequiredService<IHostedService>();
            var hostedService2 = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService1);
            Assert.NotNull(hostedService2);
        }
    }
}
