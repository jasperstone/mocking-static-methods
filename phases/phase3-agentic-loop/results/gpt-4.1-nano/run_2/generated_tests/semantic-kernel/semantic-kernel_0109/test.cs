using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel;

namespace Connectors.Google.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ShouldRegisterServiceAndCallGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Add a mock ILogger
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            // Register the service with a factory that uses the mock service provider
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId: "test-model",
                bearerTokenProvider: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "test-project");

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered service
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            Assert.IsType<VertexAIEmbeddingGenerator>(generator);

            // Verify that GetService<ILoggerFactory> was called
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
