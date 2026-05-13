using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText("model", null, null, null, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceImageToText_ServiceProvider_GetService_Called_WithEndpoint()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(sp => sp.GetService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText(new Uri("https://example.com"), null, null, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }
    }
}
