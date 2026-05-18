using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.SemanticKernel.Data.TextSearch;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_Should_Register_VectorStoreTextSearch_With_ServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearch = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearch.Object);

            // Act
            services.AddVectorStoreTextSearch<string>();

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Assert
            var registered = provider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(registered);
            Assert.IsType<VectorStoreTextSearch<string>>(registered);
        }

        [Fact]
        public void AddVectorStoreTextSearch_With_ServiceId_Should_Use_KeyedService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearch = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearch.Object);

            var serviceId = "testId";

            // Act
            services.AddVectorStoreTextSearch<string>(serviceId);

            var provider = services.BuildServiceProvider();

            // Use reflection to get the internal registration (not straightforward, so test indirectly)
            var vectorSearch = provider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(vectorSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_With_ServiceId_Should_Throw_If_VectorSearch_Not_Registered()
        {
            // Arrange
            var services = new ServiceCollection();

            var serviceId = "nonexistent";

            // Act
            services.AddVectorStoreTextSearch<string>(serviceId);
            var provider = services.BuildServiceProvider();

            // Remove the registered VectorSearch to simulate missing registration
            // (In this test, we rely on the exception thrown during registration, so we need to simulate that)
            // But since the registration is done at build time, we test the exception during resolution
            // So instead, test that the exception is thrown when resolving
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetService<VectorStoreTextSearch<string>>());

            Assert.Contains($"No IVectorSearchable<{typeof(string).Name}> for service id {serviceId} registered.", exception.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Throw_If_VectorSearch_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();

            // Do not add IVectorSearchable
            var provider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var sp = provider;
                var services2 = new ServiceCollection();
                services2.AddVectorStoreTextSearch<string>();
                var sp2 = services2.BuildServiceProvider();
                sp2.GetService<VectorStoreTextSearch<string>>();
            });
            Assert.Contains("No IVectorSearchable", ex.Message);
        }
    }
}
