using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Data.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WhenAllServicesProvided_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vectorSearchableMock = new Mock<IVectorSearchable<object>>();
            var textEmbeddingGenerationMock = new Mock<ITextEmbeddingGenerationService>();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new Mock<VectorStoreTextSearchOptions>();

            serviceProviderMock
                .Setup(sp => sp.GetService<ITextSearchStringMapper>())
                .Returns(stringMapperMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService<ITextSearchResultMapper>())
                .Returns(resultMapperMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(optionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchServiceId"))
                .Returns(vectorSearchableMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("textEmbeddingGenerationServiceId"))
                .Returns(textEmbeddingGenerationMock.Object);

            // Act
            services.AddVectorStoreTextSearch<object>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                serviceCollection: services,
                serviceProvider: serviceProviderMock.Object);

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<VectorStoreTextSearch<object>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WhenVectorSearchServiceNotProvided_ShouldThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchServiceId"))
                .Returns((IVectorSearchable<object>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchServiceId",
                    "textEmbeddingGenerationServiceId",
                    serviceCollection: services,
                    serviceProvider: serviceProviderMock.Object));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WhenTextEmbeddingGenerationServiceNotProvided_ShouldThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vectorSearchableMock = new Mock<IVectorSearchable<object>>();

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchServiceId"))
                .Returns(vectorSearchableMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("textEmbeddingGenerationServiceId"))
                .Returns((ITextEmbeddingGenerationService)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchServiceId",
                    "textEmbeddingGenerationServiceId",
                    serviceCollection: services,
                    serviceProvider: serviceProviderMock.Object));
        }
    }
}
