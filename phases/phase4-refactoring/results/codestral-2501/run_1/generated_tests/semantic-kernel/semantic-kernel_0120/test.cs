using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Http;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_ShouldRegisterService()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(Mock.Of<ILoggerFactory>());

            // Act
            serviceCollection.AddHuggingFaceEmbeddingGenerator(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddHuggingFaceEmbeddingGenerator_ShouldUseLoggerFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var model = "test-model";
            var endpoint = new Uri("https://api.huggingface.com");
            var apiKey = "test-api-key";
            var serviceId = "test-service-id";
            var httpClient = new HttpClient();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddHuggingFaceEmbeddingGenerator(model, endpoint, apiKey, serviceId, httpClient);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var service = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(service);
            loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.Once);
        }
    }
}
