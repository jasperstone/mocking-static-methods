using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_Should_Call_GetService_For_Dependencies()
        {
            // Arrange
            var services = new ServiceCollection();

            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddSingleton(stringMapperMock.Object);
            services.AddSingleton(resultMapperMock.Object);
            services.AddSingleton(options);

            var vectorSearchMock = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(vectorSearchMock.Object);

            // Register a dummy implementation for IVectorSearchable<string>
            services.AddTransient<IVectorSearchable<string>>(_ => vectorSearchMock.Object);

            // Act
            services.AddVectorStoreTextSearch<string>(
                (sp) => sp.GetService<ITextSearchStringMapper>(),
                (sp) => sp.GetService<ITextSearchResultMapper>(),
                options);

            var provider = services.BuildServiceProvider();

            // Act: resolve the registered VectorStoreTextSearch
            var registered = provider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(registered);
            // Verify that the dependencies were retrieved via GetService
            var stringMapper = provider.GetService<ITextSearchStringMapper>();
            var resultMapper = provider.GetService<ITextSearchResultMapper>();
            Assert.Equal(stringMapperMock.Object, stringMapper);
            Assert.Equal(resultMapperMock.Object, resultMapper);
        }
    }
}
