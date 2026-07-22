using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_Should_Call_GetService_For_Missing_Dependencies()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IServiceProvider to return specific services
            var serviceProviderMock = new Mock<IServiceProvider>();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Setup GetService to return mocks for specific types
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper)))
                .Returns(stringMapperMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper)))
                .Returns(resultMapperMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)))
                .Returns(options);

            // Mock IVectorSearchable<TRecord>
            var vectorSearchMock = new Mock<IVectorSearchable<string>>();
            // Setup GetService for IVectorSearchable<TRecord>
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<string>)))
                .Returns(vectorSearchMock.Object);

            // Register the mocked IServiceProvider
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            // Call the extension method
            services.AddVectorStoreTextSearch<string>();

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the registered VectorStoreTextSearch
            var registered = provider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(registered);
            // Verify that GetService was called for ITextSearchStringMapper and ITextSearchResultMapper
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.Once);
            // Verify that the vector search was retrieved
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IVectorSearchable<string>)), Times.Once);
        }
    }
}
