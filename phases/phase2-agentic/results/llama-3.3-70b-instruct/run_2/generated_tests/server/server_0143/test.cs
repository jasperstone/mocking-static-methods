using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Core;
using Bit.Core.Auth;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Models.Business;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Services.Implementations;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.Business;
using Bit.Core.Models.Data;
using Bit.Core.Models.Request;
using Bit.Core.Models.Response;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
using Bit.Core.Tools;
using Bit.Core.Tools.Models.Request;
using Bit.Core.Tools.Models.Response;
using Bit.Core.Utilities;
using Bit.Core.Vault;
using Bit.Core.Vault.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Tests
{
    public class EventIntegrationHandlerTests
    {
        [Fact]
        public async Task HandleEventAsync_ValidEvent_EventPublished()
        {
            // Arrange
            var eventMessage = new EventMessage
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = EventType.UserCreated
            };

            var organization = new Organization
            {
                Id = eventMessage.OrganizationId,
                Name = "Test Organization"
            };

            var user = new User
            {
                Id = eventMessage.UserId,
                Email = "test@example.com"
            };

            var configuration = new IntegrationConfigurationDetails
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Type = IntegrationType.Slack,
                Name = "Test Configuration",
                Description = "Test configuration description",
                Enabled = true,
                Settings = new Dictionary<string, string>
                {
                    { "slackToken", "test-slack-token" }
                }
            };

            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<IntegrationConfigurationDetails>>>();

            eventIntegrationPublisherMock
                .Setup(p => p.PublishAsync(configuration, eventMessage))
                .Verifiable();

            integrationFilterServiceMock
                .Setup(p => p.ShouldFilterAsync(configuration, eventMessage))
                .ReturnsAsync(false);

            configurationCacheMock
                .Setup(p => p.GetConfigurationsAsync(organization.Id))
                .ReturnsAsync(new List<IntegrationConfigurationDetails> { configuration });

            userRepositoryMock
                .Setup(p => p.GetUserByIdAsync(eventMessage.UserId))
                .ReturnsAsync(user);

            organizationRepositoryMock
                .Setup(p => p.GetOrganizationByIdAsync(eventMessage.OrganizationId))
                .ReturnsAsync(organization);

            var handler = new EventIntegrationHandler<IntegrationConfigurationDetails>(
                IntegrationType.Slack,
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            // Act
            await handler.HandleEventAsync(eventMessage);

            // Assert
            eventIntegrationPublisherMock.Verify(p => p.PublishAsync(configuration, eventMessage), Times.Once);
        }

        [Fact]
        public async Task HandleEventAsync_InvalidEvent_EventNotPublished()
        {
            // Arrange
            var eventMessage = new EventMessage
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EventType = EventType.UserCreated
            };

            var organization = new Organization
            {
                Id = eventMessage.OrganizationId,
                Name = "Test Organization"
            };

            var user = new User
            {
                Id = eventMessage.UserId,
                Email = "test@example.com"
            };

            var configuration = new IntegrationConfigurationDetails
            {
                Id = Guid.NewGuid(),
                OrganizationId = organization.Id,
                Type = IntegrationType.Slack,
                Name = "Test Configuration",
                Description = "Test configuration description",
                Enabled = true,
                Settings = new Dictionary<string, string>
                {
                    { "slackToken", "test-slack-token" }
                }
            };

            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<IntegrationConfigurationDetails>>>();

            eventIntegrationPublisherMock
                .Setup(p => p.PublishAsync(configuration, eventMessage))
                .Verifiable();

            integrationFilterServiceMock
                .Setup(p => p.ShouldFilterAsync(configuration, eventMessage))
                .ReturnsAsync(true);

            configurationCacheMock
                .Setup(p => p.GetConfigurationsAsync(organization.Id))
                .ReturnsAsync(new List<IntegrationConfigurationDetails> { configuration });

            userRepositoryMock
                .Setup(p => p.GetUserByIdAsync(eventMessage.UserId))
                .ReturnsAsync(user);

            organizationRepositoryMock
                .Setup(p => p.GetOrganizationByIdAsync(eventMessage.OrganizationId))
                .ReturnsAsync(organization);

            var handler = new EventIntegrationHandler<IntegrationConfigurationDetails>(
                IntegrationType.Slack,
                eventIntegrationPublisherMock.Object,
                integrationFilterServiceMock.Object,
                configurationCacheMock.Object,
                userRepositoryMock.Object,
                organizationRepositoryMock.Object,
                loggerMock.Object);

            // Act
            await handler.HandleEventAsync(eventMessage);

            // Assert
            eventIntegrationPublisherMock.Verify(p => p.PublishAsync(configuration, eventMessage), Times.Never);
        }
    }
}
