using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace VertexAI.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                               .Returns(mockLoggerFactory.Object);

            // Register the mock service provider as singleton
            services.AddSingleton(mockServiceProvider.Object);

            string modelId = "test-model";
            Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
            string location = "us-central1";
            string projectId = "test-project";

            // Register the services
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                tokenProvider,
                location,
                projectId);

            // Build the provider
            var serviceProvider = result.BuildServiceProvider();

            // Act: resolve the service to trigger the lambda
            var generator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            // Verify that GetService<ILoggerFactory>() was called
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }

    // Dummy classes to satisfy the code dependencies
    public interface IEmbeddingGenerator<TInput, TEmbedding> { }
    public class Embedding<T> { }
}
