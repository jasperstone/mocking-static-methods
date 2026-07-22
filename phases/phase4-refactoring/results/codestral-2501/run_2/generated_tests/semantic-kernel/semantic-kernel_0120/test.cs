using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_ShouldAddServiceToCollection()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "fake-api-key";
            var serviceId = "test-service";
            var httpClient = new HttpClient();
            var loggerFactory = new Mock<ILoggerFactory>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactory);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(HttpClient))).Returns(httpClient);
            var serviceProvider = serviceProviderMock.Object;

            // Act
            serviceCollection.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId, httpClient);

            // Assert
            var service = serviceCollection.BuildServiceProvider().GetService<ITextEmbeddingGenerationService>();
            Assert.NotNull(service);
        }
    }
}
