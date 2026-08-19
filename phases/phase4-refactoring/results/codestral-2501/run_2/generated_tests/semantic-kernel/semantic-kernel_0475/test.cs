using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddQdrantCollection_ShouldRegisterQdrantCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            services.AddQdrantCollection<string, object>("testCollection", clientProvider: null, optionsProvider: optionsProvider);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var collection = serviceProvider.GetRequiredService<QdrantCollection<string, object>>();
            Assert.NotNull(collection);
        }
    }
}
