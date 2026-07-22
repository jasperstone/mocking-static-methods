using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System;

namespace QdrantServiceCollectionExtensionsTests
{
    public class GetStoreOptionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenServiceProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);

            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, _ => new QdrantVectorStoreOptions());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGeneratorFromService_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);

            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenServiceReturnsNull()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }
    }
}
