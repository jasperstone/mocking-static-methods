using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
                servicesMock.Object,
                "model",
                null,
                null,
                null,
                null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
            Assert.Same(servicesMock.Object, result);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            var result = HuggingFaceServiceCollectionExtensions.AddHuggingFaceImageToText(
                servicesMock.Object,
                new Uri("http://example.com"),
                null,
                null,
                null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
            Assert.Same(servicesMock.Object, result);
        }
    }
}
