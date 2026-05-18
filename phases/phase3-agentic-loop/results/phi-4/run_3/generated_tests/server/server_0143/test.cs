using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SharedWeb.Utilities; // Ensure this matches your project's namespace

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_CallsGetRequiredServiceWithCorrectTypes()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceCollectionMock = new Mock<IServiceCollection>();

            // Set up the expected calls to GetRequiredService
            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationFilterService>())
                .Returns(new Mock<IIntegrationFilterService>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>())
                .Returns(new Mock<IIntegrationConfigurationDetailsCache>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IUserRepository>())
                .Returns(new Mock<IUserRepository>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOrganizationRepository>())
                .Returns(new Mock<IOrganizationRepository>().Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>())
                .Returns(new Mock<ILogger<EventIntegrationHandler<object>>>().Object);

            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>();
            listenerConfiguration.SetupGet(l => l.RoutingKey).Returns("test-routing-key");

            // Act
            ServiceCollectionExtensions.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(
                serviceCollectionMock.Object, listenerConfiguration.Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
        }
    }
}
