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
        public void AddHuggingFaceTextEmbeddingGeneration_CallsGetServiceForLoggerFactory()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock);

            var services = new ServiceCollection();
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddHuggingFaceTextEmbeddingGeneration(
                new Uri("http://example.com"),
                apiKey: "testApiKey",
                serviceId: "testServiceId",
                httpClient: new HttpClient());

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
