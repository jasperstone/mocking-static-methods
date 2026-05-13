using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.Data.TextSearch.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearchAsTransient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            serviceCollection.AddSingleton(mockVectorSearchable.Object);
            serviceCollection.AddSingleton(mockStringMapper.Object);
            serviceCollection.AddSingleton(mockResultMapper.Object);
            serviceCollection.AddSingleton(options);

            // Act
            serviceCollection.AddVectorStoreTextSearch<string>();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();

            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenNoIVectorSearchableRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddVectorStoreTextSearch<string>());
        }
    }
}
