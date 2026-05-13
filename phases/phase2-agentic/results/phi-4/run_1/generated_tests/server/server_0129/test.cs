using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Bit.SharedWeb.Utilities; // Assuming this is the correct namespace

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureServiceBusIntegration_CallsGetRequiredServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<EventIntegrationHandler<object>>>();

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IEventIntegrationPublisher>())
            .Returns(new Mock<IEventIntegrationPublisher>().Object);

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
            .Returns(loggerMock.Object);

        var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;

        // Act
        services.AddAzureServiceBusIntegration<object, object>(listenerConfiguration);

        // Assert
        serviceProviderMock.Verify(s => s.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationFilterService>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IIntegrationConfigurationDetailsCache>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IUserRepository>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IOrganizationRepository>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<EventIntegrationHandler<object>>>(), Times.Once);
    }
}
