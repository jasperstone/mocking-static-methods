using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
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
        public void AddEventIntegrationListener_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            var globalSettings = new Mock<GlobalSettings>();

            // Act
            services.AddEventIntegrationListener<MockConfig>(listenerConfiguration.Object, globalSettings.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IEventMessageHandler>());
            Assert.NotNull(serviceProvider.GetService<IHostedService>());
            Assert.NotNull(serviceProvider.GetService<IRabbitMqService>());
            Assert.NotNull(serviceProvider.GetService<ILoggerFactory>());
            Assert.NotNull(serviceProvider.GetService<TimeProvider>());
        }

        [Fact]
        public void AddEventIntegrationListener_ShouldThrowException_WhenListenerConfigurationIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<GlobalSettings>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddEventIntegrationListener<MockConfig>(null, globalSettings.Object));
        }

        [Fact]
        public void AddEventIntegrationListener_ShouldThrowException_WhenGlobalSettingsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddEventIntegrationListener<MockConfig>(listenerConfiguration.Object, null));
        }
    }

    public class MockConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public string IntegrationType { get; set; }
    }
}
