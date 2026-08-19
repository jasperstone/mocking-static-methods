using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using System;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_Should_Call_GetService_For_Required_Dependency()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);
            var provider = services.BuildServiceProvider();

            // Act
            services.AddVectorStoreTextSearch<string>(sp => mockVectorSearchable.Object);
            var sp = services.BuildServiceProvider();

            // Assert
            var vectorSearch = sp.GetService<IVectorSearchable<string>>();
            Assert.NotNull(vectorSearch);
        }
    }
}
