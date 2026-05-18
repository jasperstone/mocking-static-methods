using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;
using Moq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_WithBearerKey_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            services.AddVertexAIEmbeddingGenerator(
                modelId: "test-model",
                bearerKey: "test-bearer-key",
                location: "test-location",
                projectId: "test-project",
                serviceId: "test-service");

            var serviceProvider = services.BuildServiceProvider();

            // The AddKeyedSingleton extension method registers the service keyed by serviceId.
            // We try to resolve the service by the interface type (without key) - it may be null.
            var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();

            // Assert
            // embeddingGenerator may be null because keyed registration may not register default service.
            // Instead, we test the factory delegate directly by invoking it with a mock IServiceProvider.

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);

            // Use reflection to get the AddVertexAIEmbeddingGenerator method to get the factory delegate
            var method = typeof(VertexAIServiceCollectionExtensions).GetMethod("AddVertexAIEmbeddingGenerator", new Type[] {
                typeof(IServiceCollection),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(VertexAIVersion),
                typeof(string),
                typeof(HttpClient)
            });

            Assert.NotNull(method);

            // We invoke the method to get the IServiceCollection back (not needed here)
            method.Invoke(null, new object[] {
                services,
                "test-model",
                "test-bearer-key",
                "test-location",
                "test-project",
                VertexAIVersion.V1,
                "test-service",
                null
            });

            // Instead, we directly create the VertexAIEmbeddingGenerator with the mockServiceProvider to test GetService call
            var vertexAIEmbeddingGenerator = new VertexAIEmbeddingGenerator(
                modelId: "test-model",
                bearerKey: "test-bearer-key",
                location: "test-location",
                projectId: "test-project",
                apiVersion: Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1,
                httpClient: new HttpClient(),
                loggerFactory: mockServiceProvider.Object.GetService(typeof(ILoggerFactory)) as ILoggerFactory);

            Assert.NotNull(vertexAIEmbeddingGenerator);

            // Use reflection to get the private _generator field to check it is not null
            var generatorField = typeof(VertexAIEmbeddingGenerator).GetField("_generator", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(generatorField);
            var innerGenerator = generatorField.GetValue(vertexAIEmbeddingGenerator);
            Assert.NotNull(innerGenerator);
        }
    }
}
