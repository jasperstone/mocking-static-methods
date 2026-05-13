using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.SemanticKernel.Data.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WhenIVectorSearchableIsRegistered_ReturnsService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IVectorSearchable<string>, MockVectorSearchable>();
            services.AddSingleton<ITextSearchStringMapper, MockTextSearchStringMapper>();
            services.AddSingleton<ITextSearchResultMapper, MockTextSearchResultMapper>();
            services.AddSingleton<VectorStoreTextSearchOptions>();

            // Act
            services.AddVectorStoreTextSearch<string>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetRequiredService<ITextSearch>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WhenIVectorSearchableIsNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ITextSearchStringMapper, MockTextSearchStringMapper>();
            services.AddSingleton<ITextSearchResultMapper, MockTextSearchResultMapper>();
            services.AddSingleton<VectorStoreTextSearchOptions>();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                services.AddVectorStoreTextSearch<string>();
            });

            Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
        }
    }

    // Mock implementations for testing
    public class MockVectorSearchable : IVectorSearchable<string> { }
    public class MockTextSearchStringMapper : ITextSearchStringMapper { }
    public class MockTextSearchResultMapper : ITextSearchResultMapper { }
}
