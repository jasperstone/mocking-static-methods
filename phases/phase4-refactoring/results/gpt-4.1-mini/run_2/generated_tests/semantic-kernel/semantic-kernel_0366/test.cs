using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Xunit;
using Moq;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        public class DummyRecord { }

        [Fact]
        public void AddVectorStoreTextSearch_UsesGetServiceOnIServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var vectorSearchMock = new Mock<IVectorSearchable<DummyRecord>>();
            var embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();

            var spMock = new Mock<IServiceProvider>();

            // Setup GetService calls on IServiceProvider
            spMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(stringMapperMock.Object);
            spMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(resultMapperMock.Object);
            spMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(null);

            // Setup GetKeyedService extension method calls (simulate by extension method on IServiceProvider)
            spMock.Setup(sp => sp.GetKeyedService(It.IsAny<string>())).Returns((string id) =>
            {
                if (id == "vectorSearchId") return vectorSearchMock.Object;
                if (id == "embeddingServiceId") return embeddingServiceMock.Object;
                return null;
            });

            // Act
            services.AddVectorStoreTextSearch<DummyRecord>(
                "vectorSearchId",
                "embeddingServiceId",
                null,
                null,
                null,
                "myServiceId");

            // Extract the factory delegate registered by AddKeyedTransient
            var serviceDescriptor = Assert.Single(services, d => d.ServiceType == typeof(VectorStoreTextSearch<DummyRecord>));
            var factory = serviceDescriptor.ImplementationFactory;
            Assert.NotNull(factory);

            // Invoke the factory delegate with the mocked IServiceProvider
            var instance = factory!(spMock.Object) as VectorStoreTextSearch<DummyRecord>;

            // Assert
            Assert.NotNull(instance);

            // Verify that GetService was called on IServiceProvider for the optional parameters
            spMock.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.Once);
            spMock.Verify(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)), Times.Once);
        }
    }

    // Extension method to simulate GetKeyedService on IServiceProvider for mocking
    public static class ServiceProviderExtensions
    {
        public static object? GetKeyedService(this IServiceProvider sp, string key)
        {
            // This method is only for mocking purposes in the test
            throw new NotImplementedException();
        }
    }
}
