using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.ImageToText;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_WithModel_RegistersServiceAndResolvesLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            string model = "test-model";

            // Act
            services.AddHuggingFaceImageToText(model);

            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceImageToTextService>(service);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_RegistersServiceAndResolvesLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            services.AddSingleton(loggerFactoryMock.Object);

            Uri endpoint = new Uri("https://example.com");

            // Act
            services.AddHuggingFaceImageToText(endpoint);

            var provider = services.BuildServiceProvider();
            var service = provider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(service);
            Assert.IsType<HuggingFaceImageToTextService>(service);
        }
    }
}
