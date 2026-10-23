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
        public class DummyRecord { public string Text { get; set; } = string.Empty; }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceIds_RegistersVectorStoreTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();

            var vectorSearchMock = new Mock<IVectorSearchable<DummyRecord>>();
            var embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Register dependencies keyed by service id
            services.AddSingleton(vectorSearchMock.Object);
            services.AddSingleton(embeddingServiceMock.Object);
            services.AddSingleton(stringMapperMock.Object);
            services.AddSingleton(resultMapperMock.Object);
            services.AddSingleton(options);

            // Act
            services.AddVectorStoreTextSearch<DummyRecord>(
                vectorSearchServiceId: "vectorSearchId",
                textEmbeddingGenerationServiceId: "embeddingServiceId",
                stringMapper: null,
                resultMapper: null,
                options: null,
                serviceId: "myServiceId");

            // Assert
            Assert.Contains(services, sd => sd.ServiceType.Name.Contains("VectorStoreTextSearch"));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceIds_ThrowsIfVectorSearchNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddVectorStoreTextSearch<DummyRecord>(
                vectorSearchServiceId: "missingVectorSearchId",
                textEmbeddingGenerationServiceId: "embeddingServiceId");

            var provider = services.BuildServiceProvider();

            // The VectorStoreTextSearch is registered as transient keyed service, so we simulate the factory call
            var serviceDescriptor = Assert.Single(services, sd => sd.ServiceType.Name.Contains("VectorStoreTextSearch"));
            var factory = serviceDescriptor.ImplementationFactory;
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = factory(provider);
            });

            Assert.Contains("No IVectorizedSearch<TRecord> for service id missingVectorSearchId registered", ex.Message);
        }
    }
}
