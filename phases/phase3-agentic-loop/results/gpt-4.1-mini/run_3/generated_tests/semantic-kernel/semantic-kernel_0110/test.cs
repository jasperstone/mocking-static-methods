using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndCallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "us-central1";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service-id";

            // We will mock ILoggerFactory to verify that GetService is called on the service provider
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Create a mock service provider that returns the loggerFactoryMock when GetService<ILoggerFactory> is called
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);

            // Add the mock service provider to the service collection as a singleton
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                new HttpClient());

            // Build the service provider to resolve the registered service
            var builtServiceProvider = services.BuildServiceProvider();

            // Resolve the embedding generator using the serviceId key
            var embeddingGenerator = builtServiceProvider.GetService(typeof(IEmbeddingGenerator<string, Embedding<float>>));

            // Assert
            Assert.NotNull(embeddingGenerator);

            // We cannot directly verify the call to GetService on the service provider inside the factory delegate,
            // but we can verify that the service provider mock's GetService was called for ILoggerFactory.
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce);
        }
    }
}
