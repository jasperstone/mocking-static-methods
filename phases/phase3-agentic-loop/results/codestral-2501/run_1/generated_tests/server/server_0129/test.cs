using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Entities;
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
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(Mock.Of<IEventIntegrationPublisher>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(Mock.Of<IIntegrationFilterService>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(Mock.Of<IIntegrationConfigurationDetailsCache>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(Mock.Of<IUserRepository>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(Mock.Of<IOrganizationRepository>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<TestConfig>>>()).Returns(Mock.Of<ILogger<EventIntegrationHandler<TestConfig>>>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IAzureServiceBusService>()).Returns(Mock.Of<IAzureServiceBusService>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddAzureServiceBusIntegration<TestConfig, TestListenerConfig>(listenerConfiguration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();
            var hostedService = serviceProvider.GetRequiredService<IHostedService>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedService);
        }
    }

    public class TestConfig { }
    public class TestListenerConfig : IIntegrationListenerConfiguration
    {
        public string RoutingKey => "test";
        public string IntegrationType => "test";
        public int EventPrefetchCount => 1;
        public int EventMaxConcurrentCalls => 1;
        public int IntegrationPrefetchCount => 1;
        public int IntegrationMaxConcurrentCalls => 1;
    }
}
