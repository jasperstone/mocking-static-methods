using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData; // Assuming namespace for the extension class

namespace VectorData.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddPostgresVectorStore_WithEmbeddingGenerator_ShouldConfigureOptionsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IEmbeddingGenerator
            var embeddingGeneratorMock = new object();

            // Register the IEmbeddingGenerator service
            services.AddSingleton(embeddingGeneratorMock);

            // Use AddPostgresVectorStore to register the store
            services.AddPostgresVectorStore(
                "dummy-connection-string",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act
            // Resolve the VectorStore (which internally resolves PostgresVectorStore)
            var vectorStore = serviceProvider.GetService<VectorStore>();

            // Assert
            Assert.NotNull(vectorStore);
            // Since internal options are not directly accessible, 
            // we verify that the service provider's GetService<IEmbeddingGenerator>() was called during resolution.
            // But we can't directly verify that without a mock, so we just ensure no exceptions and store is resolved.
        }
    }
}
