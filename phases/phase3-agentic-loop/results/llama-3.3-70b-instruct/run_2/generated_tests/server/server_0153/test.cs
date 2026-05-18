using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddEventIntegrationListener_ConfigurationIsValid_ServiceProviderGetRequiredServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new IntegrationListenerConfiguration
            {
                RoutingKey = "test-routing-key",
                IntegrationType = "test-integration-type"
            };
            var serviceProviderMock = new Mock<IServiceProvider>();
            var eventIntegrationPublisherMock = new Mock<IEventIntegrationPublisher>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();
            var configurationCacheMock = new Mock<IIntegrationConfigurationDetailsCache>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var organizationRepositoryMock = new Mock<IOrganizationRepository>();
            var loggerMock = new Mock<ILogger<EventIntegrationHandler<GlobalSettings>>>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IEventIntegrationPublisher>()).Returns(eventIntegrationPublisherMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationFilterService>()).Returns(integrationFilterServiceMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(configurationCacheMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUserRepository>()).Returns(userRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOrganizationRepository>()).Returns(organizationRepositoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<GlobalSettings>>>()).Returns(loggerMock.Object);

            // Act
            services.AddEventIntegrationListener<GlobalSettings>(listenerConfiguration);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILogger<EventIntegrationHandler<GlobalSettings>>>(), Times.Once);
        }
    }
}
