using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_Should_Call_GetService_For_ILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy ILoggerFactory to the service collection
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            string modelId = "model123";
            string bearerKey = "key123";
            string location = "us-central1";
            string projectId = "project123";

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId);

            // Build the service provider
            var provider = result.BuildServiceProvider();

            // Act: resolve the IEmbeddingGenerator to trigger the GetService call
            var generator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(generator);
            mockLoggerFactory.Verify(lf => lf, Times.AtLeastOnce);
        }
    }
}
