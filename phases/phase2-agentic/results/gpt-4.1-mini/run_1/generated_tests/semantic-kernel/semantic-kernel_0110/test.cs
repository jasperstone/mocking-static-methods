using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            var modelId = "test-model";
            var bearerKey = "test-bearer-key";
            var location = "us-central1";
            var projectId = "test-project";
            var apiVersion = VertexAIVersion.V1;
            var serviceId = "test-service";

            // We will add a mock ILoggerFactory to the service provider to verify GetService call
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            // Add the mock ILoggerFactory to the services so it can be resolved
            services.AddSingleton(loggerFactoryMock.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                httpClient: null);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // The service provider should be able to resolve the IEmbeddingGenerator with the serviceId key
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
            Assert.NotNull(embeddingGenerator);

            // The logger factory should be resolved from the service provider (GetService call)
            var resolvedLoggerFactory = serviceProvider.GetService<ILoggerFactory>();
            Assert.NotNull(resolvedLoggerFactory);
            Assert.Same(loggerFactoryMock.Object, resolvedLoggerFactory);
        }
    }
}
