using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Http;

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
            var httpClientMock = new Mock<HttpClient>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(HttpClient)))
                .Returns(httpClientMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Mock HttpClientProvider.GetHttpClient to return the mocked HttpClient
            HttpClientProvider.GetHttpClient = (httpClient, serviceProvider) => httpClientMock.Object;

            // Act
            serviceCollection.AddHuggingFaceImageToText(
                "model",
                new Uri("https://example.com"),
                "apiKey",
                "serviceId",
                new HttpClient());

            var serviceProvider = serviceCollection.BuildServiceProvider();
            var imageToTextService = serviceProvider.GetService<IImageToTextService>();

            // Assert
            Assert.NotNull(imageToTextService);
        }
    }
}
