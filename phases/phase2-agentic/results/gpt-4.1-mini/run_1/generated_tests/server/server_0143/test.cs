using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.LoginFeatures;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
using Bit.Core.Auth.Enums;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a dummy listener configuration with required properties
            var listenerConfig = new DummyListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };

            // Setup mocks for IServiceProvider to return dummy services when GetRequiredService is called
            var serviceProviderMock = new Mock<IServiceProvider>();

            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();
            var rabbitMqServiceMock = new Mock<IRabbitMqService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var timeProviderMock = new Mock<TimeProvider>();

            // Setup GetRequiredService calls for the types expected in the method
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IRabbitMqService))).Returns(rabbitMqServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TimeProvider))).Returns(timeProviderMock.Object);

            // Act
            // Call the private extension method AddRabbitMqIntegration via reflection since it's private
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var genericMethod = methodInfo.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfiguration));
            genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Build service provider to resolve services
            var builtProvider = services.BuildServiceProvider();

            // Assert
            // Check that the services collection contains the expected registrations
            Assert.Contains(services, sd => sd.ServiceType == typeof(IEventMessageHandler));
            Assert.Contains(services, sd => sd.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService));

            // Verify that GetRequiredService was called on the service provider mock for expected types
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IRabbitMqService)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(TimeProvider)), Times.AtLeastOnce);
        }

        // Dummy classes to satisfy generic constraints and parameters
        private class DummyConfig { }
        private class DummyListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }
    }

    // Dummy interfaces to satisfy references in the tested method
    public interface IEventIntegrationPublisher { }
    public interface IIntegrationFilterService { }
    public interface IIntegrationConfigurationDetailsCache { }
    public interface IUserRepository { }
    public interface IOrganizationRepository { }
    public interface IEventMessageHandler { }
    public interface IIntegrationHandler<T> { }
    public interface IRabbitMqService { }
    public interface IIntegrationListenerConfiguration
    {
        string RoutingKey { get; }
        string IntegrationType { get; }
    }
}
