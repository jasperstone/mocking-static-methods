using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Microsoft.SemanticKernel.ImageToText;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceImageToText_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(x => x.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddHuggingFaceImageToText("model", new Uri("https://example.com"), "apiKey", "serviceId");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();

            Assert.NotNull(imageToTextService);
        }
    }
}
