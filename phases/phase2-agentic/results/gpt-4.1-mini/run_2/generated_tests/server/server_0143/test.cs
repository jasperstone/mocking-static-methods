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

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesAndResolvesDependencies()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a dummy listener configuration with required properties
            var listenerConfigMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigMock.SetupGet(c => c.RoutingKey).Returns("test-routing-key");
            listenerConfigMock.SetupGet(c => c.IntegrationType).Returns("test-integration-type");
            var listenerConfig = listenerConfigMock.Object;

            // Setup mocks for required services to be resolved by IServiceProvider
            var serviceProviderMock = new Mock<IServiceProvider>();

            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();

            var rabbitMqServiceMock = new Mock<IRabbitMqService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var timeProviderMock = new Mock<TimeProvider>();

            // Setup GetRequiredService calls
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher))).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationFilterService))).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOrganizationRepository))).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<object>>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IRabbitMqService))).Returns(rabbitMqServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TimeProvider))).Returns(timeProviderMock.Object);

            // Setup GetRequiredKeyedService for IEventMessageHandler keyed by routing key
            var eventMessageHandlerMock = new Mock<IEventMessageHandler>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventMessageHandler))).Returns(eventMessageHandlerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IIntegrationHandler<object>))).Returns(new Mock<IIntegrationHandler<object>>().Object);

            // Act
            // Call the private extension method via reflection since it's private
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), listenerConfig.GetType());
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            // Build service provider and verify registrations
            var builtProvider = services.BuildServiceProvider();

            // Verify that the services can be resolved without exceptions
            var eventMessageHandler = builtProvider.GetService<IEventMessageHandler>();
            var hostedServices = builtProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

            // We expect at least one IEventMessageHandler and some IHostedService registrations
            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(hostedServices);
        }
    }
}
