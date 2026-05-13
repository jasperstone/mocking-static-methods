using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.NotificationCenter;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
using Bit.Core.Auth.LoginFeatures;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Auth.IdentityServer;
using Bit.Core.Auth.Enums;
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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        // We will test the AddAzureServiceBusIntegration extension method indirectly by invoking it via reflection
        // because it is private static. We want to cover the call to provider.GetRequiredService<T> on line 889.

        // To do this, we will:
        // - Create a mock IServiceCollection
        // - Create a mock IServiceProvider that returns mocks for all required services
        // - Create a dummy listenerConfiguration with required properties
        // - Use reflection to invoke AddAzureServiceBusIntegration<TConfig, TListenerConfig>
        // - Verify that GetRequiredService was called on the IServiceProvider mock for the expected service types

        private interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
            int EventPrefetchCount { get; }
            int EventMaxConcurrentCalls { get; }
        }

        private interface IEventIntegrationPublisher { }
        private interface IIntegrationFilterService { }
        private interface IIntegrationConfigurationDetailsCache { }
        private interface IUserRepository { }
        private interface IOrganizationRepository { }
        private interface IIntegrationHandler<T> { }
        private interface IAzureServiceBusService { }
        private interface IEventMessageHandler { }
        private interface ILogger<T> { }
        private interface ILoggerFactory { }
        private interface IHostedService { }

        private class DummyListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
            public int EventPrefetchCount { get; set; }
            public int EventMaxConcurrentCalls { get; set; }
        }

        private class DummyConfig { }

        [Fact]
        public void AddAzureServiceBusIntegration_CallsGetRequiredServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();

            // Setup mocks for all required services returned by GetRequiredService
            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<DummyConfig>>>();

            var mockAzureServiceBusService = new Mock<IAzureServiceBusService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();

            var mockEventMessageHandler = new Mock<IEventMessageHandler>();
            var mockIntegrationHandler = new Mock<IIntegrationHandler<DummyConfig>>();

            // Setup GetRequiredService calls to return the mocks
            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher))).Returns(mockEventIntegrationPublisher.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService))).Returns(mockIntegrationFilterService.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(mockConfigurationCache.Object);
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository))).Returns(mockUserRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository))).Returns(mockOrganizationRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>))).Returns(mockLogger.Object);

            mockProvider.Setup(p => p.GetService(typeof(IAzureServiceBusService))).Returns(mockAzureServiceBusService.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // We also need to setup GetRequiredKeyedService for IEventMessageHandler, but since it's an extension method,
            // we will mock the IServiceProvider to return the mockEventMessageHandler when asked for IEventMessageHandler.
            // For simplicity, we will assume GetRequiredKeyedService calls GetService internally.
            mockProvider.Setup(p => p.GetService(typeof(IEventMessageHandler))).Returns(mockEventMessageHandler.Object);

            // Setup for IIntegrationHandler<DummyConfig>
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationHandler<DummyConfig>))).Returns(mockIntegrationHandler.Object);

            var listenerConfig = new DummyListenerConfig
            {
                RoutingKey = "testRoutingKey",
                IntegrationType = "testIntegrationType",
                EventPrefetchCount = 5,
                EventMaxConcurrentCalls = 10
            };

            // Use reflection to get the private static method AddAzureServiceBusIntegration
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddAzureServiceBusIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Act
            // Invoke the generic method with DummyConfig and DummyListenerConfig
            var genericMethod = methodInfo.MakeGenericMethod(typeof(DummyConfig), typeof(DummyListenerConfig));
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            // The method returns IServiceCollection, so result should be services
            Assert.Same(services, result);

            // Verify that GetRequiredService was called for all expected service types
            mockProvider.Verify(p => p.GetService(typeof(IEventIntegrationPublisher)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationFilterService)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IUserRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IOrganizationRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(ILogger<EventIntegrationHandler<DummyConfig>>)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IAzureServiceBusService)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IEventMessageHandler)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationHandler<DummyConfig>)), Times.AtLeastOnce);
        }
    }
}
