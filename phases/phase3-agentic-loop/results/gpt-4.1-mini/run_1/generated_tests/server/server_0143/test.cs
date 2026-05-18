using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Data.EventIntegrations;
using Bit.Core.AdminConsole.Services;
using Bit.Core.Auth.Services;
using Bit.Core.Repositories;
using Bit.Core.Platform;
using Bit.Core.Services;
using Microsoft.Extensions.Hosting;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; } = "test-routing-key";
            public string IntegrationType { get; set; } = "test-integration-type";
        }

        [Fact]
        public void AddRabbitMqIntegration_RegistersServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();

            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();

            var mockRabbitMqService = new Mock<IRabbitMqService>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockTimeProvider = new Mock<TimeProvider>();

            mockProvider.Setup(p => p.GetService(typeof(IEventIntegrationPublisher))).Returns(mockEventIntegrationPublisher.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationFilterService))).Returns(mockIntegrationFilterService.Object);
            mockProvider.Setup(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache))).Returns(mockConfigurationCache.Object);
            mockProvider.Setup(p => p.GetService(typeof(IUserRepository))).Returns(mockUserRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(IOrganizationRepository))).Returns(mockOrganizationRepository.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILogger<EventIntegrationHandler<object>>))).Returns(mockLogger.Object);

            mockProvider.Setup(p => p.GetService(typeof(IRabbitMqService))).Returns(mockRabbitMqService.Object);
            mockProvider.Setup(p => p.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockProvider.Setup(p => p.GetService(typeof(TimeProvider))).Returns(mockTimeProvider.Object);

            var listenerConfig = new TestListenerConfig();

            // Act
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), typeof(TestListenerConfig));

            var result = genericMethod.Invoke(null, new object[] { services, listenerConfig });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            bool hasEventMessageHandler = services.Any(sd => sd.ServiceType == typeof(IEventMessageHandler));
            bool hasHostedService = services.Any(sd => sd.ServiceType == typeof(IHostedService));

            Assert.True(hasEventMessageHandler, "IEventMessageHandler should be registered.");
            Assert.True(hasHostedService, "IHostedService should be registered.");

            var eventMessageHandlerDescriptor = services.FirstOrDefault(sd => sd.ServiceType == typeof(IEventMessageHandler));
            Assert.NotNull(eventMessageHandlerDescriptor);
            Assert.NotNull(eventMessageHandlerDescriptor.ImplementationFactory);

            var handlerInstance = eventMessageHandlerDescriptor.ImplementationFactory(mockProvider.Object);
            Assert.NotNull(handlerInstance);

            mockProvider.Verify(p => p.GetService(typeof(IEventIntegrationPublisher)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationFilterService)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IUserRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(IOrganizationRepository)), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetService(typeof(ILogger<EventIntegrationHandler<object>>)), Times.AtLeastOnce);
        }
    }
}
