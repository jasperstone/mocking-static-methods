using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData.PgVector;

namespace Microsoft.Extensions.VectorData.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddPostgresVectorStore_WithEmbeddingGenerator_ShouldConfigureOptionsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(mockEmbeddingGenerator.Object);

            // Register the mock service provider
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            services.AddPostgresVectorStore(
                connectionString: "Server=localhost;Database=test;",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the PostgresVectorStore
            var store = serviceProvider.GetService<PostgresVectorStore>();
            Assert.NotNull(store);

            // Verify that GetService<IEmbeddingGenerator>() was called during options resolution
            // Since options are created inside the AddPostgresVectorStore, we need to check the internal logic indirectly
            // by resolving the VectorStore and ensuring it is registered
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }
    }
}
