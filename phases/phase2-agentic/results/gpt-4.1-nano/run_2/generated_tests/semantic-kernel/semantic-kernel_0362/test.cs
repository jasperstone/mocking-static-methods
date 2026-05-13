using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.SemanticKernel.Data.TextSearch;
using Moq;

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

            // Register a service collection with no IVectorSearchable<TRecord>
            var sp = services.BuildServiceProvider();

            // Use reflection to call the method with a dummy TRecord
            var method = typeof(TextSearchServiceCollectionExtensions)
                .GetMethod("AddVectorStoreTextSearch", new[] { typeof(IServiceCollection), typeof(string), typeof(ITextSearchStringMapper), typeof(ITextSearchResultMapper), typeof(VectorStoreTextSearchOptions), typeof(string) });

            var genericMethod = method.MakeGenericMethod(typeof(string));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                genericMethod.Invoke(null, new object[] { services, "testId" , null, null, null, null })
            );
            Assert.Contains("No IVectorSearchable<TRecord> for service id testId registered.", exception.InnerException.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Return_ServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);

            // Register the IVectorSearchable<string> with a key
            services.AddKeyedTransient<IVectorSearchable<string>>("testKey", (sp, obj) => mockVectorSearchable.Object);

            // Act
            var result = services.AddVectorStoreTextSearch<string>("testKey");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ServiceCollection>(result);
        }
    }
}
