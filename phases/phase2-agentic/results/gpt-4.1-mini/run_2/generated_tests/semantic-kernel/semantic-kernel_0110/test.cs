using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;

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

            // We will track if GetService<ILoggerFactory> is called by mocking IServiceProvider
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object)
                .Verifiable();

            // Add a factory to the service collection that uses the mocked service provider
            services.AddKeyedSingleton<IEmbeddingGenerator<string, Embedding<float>>>(serviceId, (sp, _) =>
            {
                // Use the mocked service provider to simulate the call to GetService<ILoggerFactory>
                var loggerFactory = serviceProviderMock.Object.GetService<ILoggerFactory>();
                return new VertexAIEmbeddingGenerator(
                    modelId: modelId,
                    bearerKey: bearerKey,
                    location: location,
                    projectId: projectId,
                    apiVersion: apiVersion,
                    httpClient: new HttpClient(),
                    loggerFactory: loggerFactory);
            });

            // Act
            var serviceCollection = services.AddVertexAIEmbeddingGenerator(
                modelId,
                bearerKey,
                location,
                projectId,
                apiVersion,
                serviceId,
                new HttpClient());

            // Build the service provider and resolve the embedding generator
            var provider = serviceCollection.BuildServiceProvider();
            var embeddingGenerator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            Assert.NotNull(embeddingGenerator);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
        }
    }
}
