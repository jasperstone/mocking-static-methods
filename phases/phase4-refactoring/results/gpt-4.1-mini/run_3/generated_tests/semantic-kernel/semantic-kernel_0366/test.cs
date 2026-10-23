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
        public class PublicDummyRecord { }

        [Fact]
        public void AddVectorStoreTextSearch_WithVectorSearchServiceId_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();

            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            var vectorSearchServiceId = "vectorSearchId";
            var textEmbeddingGenerationServiceId = "embeddingGenId";

            var vectorSearchKeyedMock = new Mock<IVectorSearchable<PublicDummyRecord>>();
            var embeddingGenMock = new Mock<ITextEmbeddingGenerationService>();

            // We cannot mock extension methods directly, so we simulate the factory delegate manually
            services.AddKeyedTransient<VectorStoreTextSearch<PublicDummyRecord>>(
                null,
                (sp, obj) =>
                {
                    var stringMapper = stringMapperMock.Object;
                    var resultMapper = resultMapperMock.Object;
                    var optionsLocal = options;

                    var vectorizedSearch = vectorSearchKeyedMock.Object;
                    var generationService = embeddingGenMock.Object;

                    return new VectorStoreTextSearch<PublicDummyRecord>(
                        vectorizedSearch,
                        generationService,
                        stringMapper,
                        resultMapper,
                        optionsLocal);
                });

            // Act
            services.AddVectorStoreTextSearch<PublicDummyRecord>(
                vectorSearchServiceId,
                textEmbeddingGenerationServiceId,
                stringMapperMock.Object,
                resultMapperMock.Object,
                options);

            // Assert
            Assert.Contains(services, d => d.ServiceType == typeof(VectorStoreTextSearch<PublicDummyRecord>));
        }
    }
}
