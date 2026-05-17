using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using Bit.Core; // Assuming EventIntegrationHandler<TConfig> is in this namespace
using Bit.Core.Repositories; // Assuming IUserRepository and IOrganizationRepository are here
using Bit.Core.Services; // Assuming IIntegrationFilterService and IIntegrationConfigurationDetailsCache are here
using Microsoft.Extensions.Logging; // Ensure ILogger is correctly referenced

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

            // Act
            var result = ServiceCollectionExtensions.AddRabbitMqIntegration<object, object>(serviceCollectionMock.Object, new Mock<IIntegrationListenerConfiguration>().Object);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);

            // Verify that the result is the same service collection
            Assert.Same(serviceCollectionMock.Object, result);
        }
    }
}
