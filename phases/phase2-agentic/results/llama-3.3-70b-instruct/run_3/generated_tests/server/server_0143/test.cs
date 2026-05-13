using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Auth.Models.Business;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Models.Business;
using Bit.Core.Models.Data;
using Bit.Core.Models.Response;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Tools;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Services.Implementations.Tests
{
    public class EventIntegrationHandlerTests
    {
        [Fact]
        public async Task HandleEventAsync_ValidRequest_PublishesEvents()
        {
            // Arrange
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<TestIntegrationConfigurationDetails>>>();

            var handler = new EventIntegrationHandler<TestIntegrationConfigurationDetails>(
                "TestIntegration",
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            var request = new EventIntegrationRequest
            {
                OrganizationId = 1,
                UserId = 1,
                IntegrationId = 1,
                Events = new List<Event>
                {
                    new Event { Id = 1, Type = EventType.Created },
                    new Event { Id = 2, Type = EventType.Updated }
                }
            };

            var organization = new Organization { Id = 1 };
            var user = new User { Id = 1 };
            var configuration = new TestIntegrationConfigurationDetails { Id = 1 };

            organizationRepositoryMock.Setup(o => o.GetOrganizationByIdAsync(request.OrganizationId)).ReturnsAsync(organization);
            userRepositoryMock.Setup(u => u.GetUserByIdAsync(request.UserId)).ReturnsAsync(user);
            configurationCacheMock.Setup(c => c.GetConfigurationAsync<TestIntegrationConfigurationDetails>(request.OrganizationId, request.IntegrationId)).ReturnsAsync(configuration);
            integrationFilterServiceMock.Setup(f => f.FilterEventsAsync(request.Events, configuration)).ReturnsAsync(request.Events);

            // Act
            await handler.HandleEventAsync(request);

            // Assert
            eventIntegrationPublisherMock.Verify(p => p.PublishEventsAsync(organization, user, request.Events, configuration), Times.Once);
        }

        [Fact]
        public async Task HandleEventAsync_InvalidRequest_DoesNotPublishEvents()
        {
            // Arrange
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<TestIntegrationConfigurationDetails>>>();

            var handler = new EventIntegrationHandler<TestIntegrationConfigurationDetails>(
                "TestIntegration",
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            var request = new EventIntegrationRequest
            {
                OrganizationId = 1,
                UserId = 1,
                IntegrationId = 1,
                Events = new List<Event>
                {
                    new Event { Id = 1, Type = EventType.Created },
                    new Event { Id = 2, Type = EventType.Updated }
                }
            };

            var organization = new Organization { Id = 1 };
            var user = new User { Id = 1 };
            var configuration = new TestIntegrationConfigurationDetails { Id = 1 };

            organizationRepositoryMock.Setup(o => o.GetOrganizationByIdAsync(request.OrganizationId)).ReturnsAsync((Organization)null);
            userRepositoryMock.Setup(u => u.GetUserByIdAsync(request.UserId)).ReturnsAsync(user);
            configurationCacheMock.Setup(c => c.GetConfigurationAsync<TestIntegrationConfigurationDetails>(request.OrganizationId, request.IntegrationId)).ReturnsAsync(configuration);
            integrationFilterServiceMock.Setup(f => f.FilterEventsAsync(request.Events, configuration)).ReturnsAsync(request.Events);

            // Act
            await handler.HandleEventAsync(request);

            // Assert
            eventIntegrationPublisherMock.Verify(p => p.PublishEventsAsync(It.IsAny<Organization>(), It.IsAny<User>(), It.IsAny<List<Event>>(), It.IsAny<IntegrationConfigurationDetails>()), Times.Never);
        }
    }

    public class TestIntegrationConfigurationDetails : IntegrationConfigurationDetails
    {
        public int Id { get; set; }
    }
}
