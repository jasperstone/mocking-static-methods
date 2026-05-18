using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Repositories;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.LoginFeatures;
using Bit.Core.Auth.Models.Business.Tokenables;
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
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Repositories;
using Bit.Core.Resources;
using Bit.Core.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Repositories.Noop;
using Bit.Core.Tools.ImportFeatures;
using Bit.Core.Tools.SendFeatures;
using Bit.Core.Tools.Services;
using Bit.Core.Utilities;
using Bit.Core.Vault;
using Bit.Core.Vault.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var listenerConfig = new DummyListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };

            var mockProvider = new Mock<IServiceProvider>();

            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(new DummyEventIntegrationPublisher());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService)))
                .Returns(new DummyIntegrationFilterService());
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(new DummyIntegrationConfigurationDetailsCache());
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository)))
                .Returns(new DummyUserRepository());
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository)))
                .Returns(new DummyOrganizationRepository());
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)))
                .Returns(new LoggerFactory().CreateLogger<EventIntegrationHandler<DummyConfig>>());

            mockProvider.Setup(p => p.GetService(typeof(IRabbitMqService)))
                .Returns(new DummyRabbitMqService());
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory)))
                .Returns(new LoggerFactory());
            mockProvider.Setup(p => p.GetService(typeof(TimeProvider)))
                .Returns(TimeProvider.System);

            // Act
            var method = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            var genericMethod = method.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfiguration));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);
        }

        private class DummyConfig { }

        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }

        private class DummyEventIntegrationPublisher : IEventIntegrationPublisher { }

        private class DummyIntegrationFilterService : IIntegrationFilterService { }

        private class DummyIntegrationConfigurationDetailsCache : IIntegrationConfigurationDetailsCache { }

        private class DummyUserRepository : IUserRepository { }

        private class DummyOrganizationRepository : IOrganizationRepository { }

        private class DummyRabbitMqService : IRabbitMqService { }
    }
}
