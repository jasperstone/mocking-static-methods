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
        public void AddVectorStoreTextSearch_Should_Call_GetService_For_Required_Dependencies()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();

            services.AddTransient(_ => mockVectorSearchable.Object);

            // Act
            services.AddVectorStoreTextSearch<string>(
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                    Assert.NotNull(vectorSearch);
                    return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
                });

            var sp = services.BuildServiceProvider();

            // Assert
            var service = sp.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_With_ServiceId_Should_Call_GetKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<int>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockKeyedServices = new Mock<IKeyedServiceProvider>();

            mockKeyedServices.Setup(k => k.GetKeyedService<IVectorSearchable<int>>("testId"))
                .Returns(mockVectorSearchable.Object);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IKeyedServiceProvider)))
                .Returns(mockKeyedServices.Object);

            services.AddTransient(_ => mockVectorSearchable.Object);

            // Act
            services.AddVectorStoreTextSearch<int>(
                "testId",
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetKeyedService<IVectorSearchable<int>>("testId");
                    Assert.NotNull(vectorSearch);
                    return new VectorStoreTextSearch<int>(vectorSearch, null, null, null);
                });

            var sp = services.BuildServiceProvider();

            // Assert
            var service = sp.GetService<VectorStoreTextSearch<int>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Throw_When_No_VectorSearchRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // No registration for IVectorSearchable

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    (sp, obj) =>
                    {
                        var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                        if (vectorSearch == null)
                        {
                            throw new InvalidOperationException("No IVectorSearchable registered");
                        }
                        return new VectorStoreTextSearch<string>(vectorSearch, null, null, null);
                    }));
        }
    }
}
