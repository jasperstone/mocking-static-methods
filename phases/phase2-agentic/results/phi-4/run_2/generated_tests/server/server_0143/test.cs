using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_CallsGetRequiredServiceForIntegrationFilterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var integrationFilterServiceMock = new Mock<IIntegrationFilterService>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IIntegrationFilterService>())
                .Returns(integrationFilterServiceMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            var listenerConfigurationMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigurationMock.SetupGet(c => c.RoutingKey).Returns("test-routing-key");

            // Act
            services.AddRabbitMqIntegration<object, IIntegrationListenerConfiguration>(listenerConfigurationMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IIntegrationFilterService>(), Times.Once);
        }
    }
}
