using System;
using System.Net.Http;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText("model");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void AddHuggingFaceImageToText_InitializesHuggingFaceImageToTextServiceCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(HttpClient)))
                .Returns(httpClientMock.Object);

            // Act
            services.AddHuggingFaceImageToText("model");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetServices<IImageToTextService>().First();

            Assert.IsType<HuggingFaceImageToTextService>(service);
            Assert.Equal("model", ((HuggingFaceImageToTextService)service)._client.ModelId);
            Assert.Same(httpClientMock.Object, ((HuggingFaceImageToTextService)service)._client.HttpClient);
            Assert.Same(loggerFactoryMock.Object, ((HuggingFaceImageToTextService)service)._client.Logger);
        }
    }
}
