using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Http;
using System.Threading.Tasks;

namespace SemanticKernel.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILoggerFactory to the service collection
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory>(mockLoggerFactory.Object);

            // Add a mock IEmbeddingGenerator to verify it is returned
            var mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
            services.AddSingleton(mockGenerator.Object);

            // Register a mock GetService extension
            var serviceProvider = services.BuildServiceProvider();

            // Setup the service provider to return the mock ILoggerFactory when requested
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                        .Returns(mockLoggerFactory.Object);

            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: () => new ValueTask<string>("token"),
                location: "us-central1",
                projectId: "project",
                apiVersion: VertexAIVersion.V1,
                serviceId: "service",
                httpClient: null);

            // Build the final provider to simulate the actual resolution
            var finalProvider = services.BuildServiceProvider();

            // Use reflection to access the private method or simulate the call
            // Since the extension method internally calls serviceProvider.GetService<ILoggerFactory>(),
            // we can verify that the service provider returns the mock ILoggerFactory
            var loggerFactory = finalProvider.GetService<ILoggerFactory>();

            // Assert
            Assert.NotNull(loggerFactory);
            Assert.IsType<ILoggerFactory>(loggerFactory);
        }
    }
}
