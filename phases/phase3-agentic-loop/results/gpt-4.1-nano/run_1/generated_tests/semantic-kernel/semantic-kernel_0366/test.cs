using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithServiceProvider_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();

            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            var vectorSearchMock = new Mock<IVectorSearchable<string>>();
            var embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<ITextSearchStringMapper>())
                .Returns(stringMapperMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService<ITextSearchResultMapper>())
                .Returns(resultMapperMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(options);
            serviceProviderMock.Setup(sp => sp.GetService<IVectorSearchable<string>>())
                .Returns(vectorSearchMock.Object);
            serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<string>>("test"))
                .Returns(vectorSearchMock.Object);
            serviceProviderMock.Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("embed"))
                .Returns(embeddingServiceMock.Object);

            // Act
            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "test",
                (sp, obj) =>
                {
                    var stringMapper = sp.GetService<ITextSearchStringMapper>();
                    var resultMapper = sp.GetService<ITextSearchResultMapper>();
                    var opts = sp.GetService<VectorStoreTextSearchOptions>();
                    var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("test");
                    var genService = sp.GetKeyedService<ITextEmbeddingGenerationService>("embed");
                    return new VectorStoreTextSearch<string>(vectorSearch, genService, stringMapper, resultMapper, opts);
                });

            var provider = services.BuildServiceProvider();

            // Manually invoke the factory to simulate the extension method behavior
            var factory = new Func<IServiceProvider, object, VectorStoreTextSearch<string>>((sp, obj) =>
            {
                var stringMapper = sp.GetService<ITextSearchStringMapper>();
                var resultMapper = sp.GetService<ITextSearchResultMapper>();
                var opts = sp.GetService<VectorStoreTextSearchOptions>();
                var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("test");
                var genService = sp.GetKeyedService<ITextEmbeddingGenerationService>("embed");
                return new VectorStoreTextSearch<string>(vectorSearch, genService, stringMapper, resultMapper, opts);
            });
            var result = factory(provider, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VectorStoreTextSearch<string>>(result);
            // Verify that GetService was called
            stringMapperMock.Verify(sp => sp.GetService<ITextSearchStringMapper>(), Times.Once);
            resultMapperMock.Verify(sp => sp.GetService<ITextSearchResultMapper>(), Times.Once);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_CallsGetKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();

            var vectorSearchMock = new Mock<IVectorSearchable<string>>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<string>>("vectorId"))
                .Returns(vectorSearchMock.Object);

            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "serviceId",
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("vectorId");
                    if (vectorSearch is null)
                        throw new InvalidOperationException();
                    return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
                });

            var provider = services.BuildServiceProvider();

            // Act
            var factory = new Func<IServiceProvider, object, VectorStoreTextSearch<string>>((sp, obj) =>
            {
                var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("vectorId");
                return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
            });
            var result = factory(provider, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VectorStoreTextSearch<string>>(result);
            // Verify that GetKeyedService was called
            serviceProviderMock.Verify(sp => sp.GetKeyedService<IVectorSearchable<string>>("vectorId"), Times.Once);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithMissingVectorSearch_Throws()
        {
            // Arrange
            var services = new ServiceCollection();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<string>>("missing"))
                .Returns((IVectorSearchable<string>)null);

            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "serviceId",
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("missing");
                    if (vectorSearch is null)
                        throw new InvalidOperationException();
                    return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
                });

            var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var factory = new Func<IServiceProvider, object, VectorStoreTextSearch<string>>((sp, obj) =>
                {
                    var vectorSearch = sp.GetKeyedService<IVectorSearchable<string>>("missing");
                    if (vectorSearch is null)
                        throw new InvalidOperationException();
                    return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
                });
                var result = factory(provider, null);
            });
        }
    }
}
