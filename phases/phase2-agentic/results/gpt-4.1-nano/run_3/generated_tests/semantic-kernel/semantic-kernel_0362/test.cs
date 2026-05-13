using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Data.TextSearch;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_Should_Throw_When_IVectorSearchable_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var provider = services.BuildServiceProvider();

            // Register a service collection without IVectorSearchable
            var sp = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                    "test",
                    (sp, obj) =>
                    {
                        var stringMapper = sp.GetService<ITextSearchStringMapper>();
                        var resultMapper = sp.GetService<ITextSearchResultMapper>();
                        var options = sp.GetService<VectorStoreTextSearchOptions>();
                        var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                        return vectorSearch == null
                            ? throw new InvalidOperationException("No IVectorSearchable registered.")
                            : new VectorStoreTextSearch<string>(vectorSearch, stringMapper, resultMapper, options);
                    }));
        }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Use_GetService_For_IVectorSearchable()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);

            // Register dependencies
            services.AddTransient<ITextSearchStringMapper, MockTextSearchStringMapper>();
            services.AddTransient<ITextSearchResultMapper, MockTextSearchResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, VectorStoreTextSearchOptions>();

            // Act
            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "test",
                (sp, obj) =>
                {
                    var stringMapper = sp.GetService<ITextSearchStringMapper>();
                    var resultMapper = sp.GetService<ITextSearchResultMapper>();
                    var options = sp.GetService<VectorStoreTextSearchOptions>();
                    var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                    return vectorSearch == null
                        ? throw new InvalidOperationException("No IVectorSearchable registered.")
                        : new VectorStoreTextSearch<string>(vectorSearch, stringMapper, resultMapper, options);
                });

            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(service);
            Assert.Equal(mockVectorSearchable.Object, service.VectorSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_With_ServiceId_Should_Use_GetKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);

            // Register dependencies
            services.AddTransient<ITextSearchStringMapper, MockTextSearchStringMapper>();
            services.AddTransient<ITextSearchResultMapper, MockTextSearchResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, VectorStoreTextSearchOptions>();

            var serviceId = "myServiceId";

            // Register a mock for GetKeyedService
            services.AddTransient<Func<string, object>>(sp => key =>
            {
                if (key == serviceId)
                {
                    return mockVectorSearchable.Object;
                }
                return null;
            });

            // Act
            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                serviceId,
                (sp, obj) =>
                {
                    var stringMapper = sp.GetService<ITextSearchStringMapper>();
                    var resultMapper = sp.GetService<ITextSearchResultMapper>();
                    var options = sp.GetService<VectorStoreTextSearchOptions>();
                    var getKeyedService = sp.GetService<Func<string, object>>();
                    var vectorSearch = getKeyedService(serviceId) as IVectorSearchable<string>;
                    if (vectorSearch == null)
                        throw new InvalidOperationException($"No IVectorSearchable for service id {serviceId} registered.");
                    return new VectorStoreTextSearch<string>(vectorSearch, stringMapper, resultMapper, options);
                });

            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(service);
            Assert.Equal(mockVectorSearchable.Object, service.VectorSearch);
        }
    }

    // Mock implementations for dependencies
    public class MockTextSearchStringMapper : ITextSearchStringMapper { }
    public class MockTextSearchResultMapper : ITextSearchResultMapper { }
}
