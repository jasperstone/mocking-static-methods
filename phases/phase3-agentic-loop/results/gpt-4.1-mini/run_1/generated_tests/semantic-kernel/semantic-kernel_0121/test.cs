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
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText(
                "test-model",
                new Uri("http://localhost"),
                "test-api-key",
                "testService",
                new HttpClient());

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the service to trigger the factory delegate and the GetService call
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(imageToTextService);
        }

        [Fact]
        public void AddHuggingFaceImageToText_WithEndpoint_RegistersServiceAndResolvesLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddHuggingFaceImageToText(
                new Uri("http://localhost"),
                "test-api-key",
                "testService",
                new HttpClient());

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the service to trigger the factory delegate and the GetService call
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(imageToTextService);
        }
    }
}
