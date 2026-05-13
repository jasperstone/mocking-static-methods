using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.HostedServices;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Platform;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.NotificationCenter;
using Bit.Core.KeyManagement;
using Bit.Core.OrganizationFeatures;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_RegistersServicesAndCallsGetRequiredService()
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

            // Setup GetRequiredService calls on the provider mock
            mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(mockEventIntegrationPublisher.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(mockIntegrationFilterService.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(mockConfigurationCache.Object);
            mockProvider.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(mockUserRepository.Object);
            mockProvider.Setup(p => p.GetRequiredService<IOrganizationRepository>()).Returns(mockOrganizationRepository.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(mockLogger.Object);
            mockProvider.Setup(p => p.GetRequiredService<IRabbitMqService>()).Returns(mockRabbitMqService.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockProvider.Setup(p => p.GetRequiredService<TimeProvider>()).Returns(mockTimeProvider.Object);

            // Setup GetRequiredKeyedService for IEventMessageHandler
            var mockEventMessageHandler = new Mock<IEventMessageHandler>();
            mockProvider.Setup(p => p.GetRequiredKeyedService<IEventMessageHandler>(It.IsAny<string>())).Returns(mockEventMessageHandler.Object);

            // Create a listener configuration mock
            var listenerConfigMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigMock.SetupGet(l => l.RoutingKey).Returns("routingKey");
            listenerConfigMock.SetupGet(l => l.IntegrationType).Returns("integrationType");

            // Act
            // We need to call the private extension method AddRabbitMqIntegration via reflection because it's private
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod("AddRabbitMqIntegration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            var genericMethod = methodInfo.MakeGenericMethod(typeof(object), listenerConfigMock.Object.GetType());
            var result = genericMethod.Invoke(null, new object[] { services, listenerConfigMock.Object });

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IServiceCollection>(result);

            // Verify that GetRequiredService was called on the provider mock for expected services
            mockProvider.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<IUserRepository>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<IOrganizationRepository>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<IRabbitMqService>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredService<TimeProvider>(), Times.AtLeastOnce);
            mockProvider.Verify(p => p.GetRequiredKeyedService<IEventMessageHandler>(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
