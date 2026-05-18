using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithNullVectorSearchable_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<object>());
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithValidVectorSearchable_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IVectorSearchable<object>, MockVectorSearchable>();

            // Act and Assert
            services.AddVectorStoreTextSearch<object>();
        }

        private class MockVectorSearchable : IVectorSearchable<object>
        {
            public Task<SearchResult<object>> SearchAsync(string query, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new SearchResult<object>
                {
                    Score = 1.0f,
                    Record = new object()
                });
            }
        }

        private class SearchResult<TRecord> : IVectorSearchable<TRecord>.SearchResult
        {
            public float Score { get; set; }
            public TRecord Record { get; set; }
        }
    }
}
