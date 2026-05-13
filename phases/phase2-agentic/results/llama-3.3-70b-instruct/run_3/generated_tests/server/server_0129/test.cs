using Xunit;
using Moq;
using System.Threading.Tasks;
using Bit.Core.Entities;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class EventIntegrationHandlerTests
    {
        [Fact]
        public async Task HandleEventAsync_ValidEventMessage_EventPublished()
        {
            // Arrange
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<TestConfig>>>();

            var eventIntegrationHandler = new EventIntegrationHandler<TestConfig>(
                "testIntegrationType",
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            var eventMessage = new EventMessage();

            // Act
            await eventIntegrationHandler.HandleEventAsync(eventMessage);

            // Assert
            eventIntegrationPublisherMock.Verify(p => p.PublishEventAsync(It.IsAny<EventMessage>()), Times.Once);
        }

        [Fact]
        public void Constructor_ValidParameters_EventIntegrationHandlerCreated()
        {
            // Arrange
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<TestConfig>>>();

            // Act
            var eventIntegrationHandler = new EventIntegrationHandler<TestConfig>(
                "testIntegrationType",
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            // Assert
            Assert.NotNull(eventIntegrationHandler);
        }

        private class TestConfig { }
    }
}
