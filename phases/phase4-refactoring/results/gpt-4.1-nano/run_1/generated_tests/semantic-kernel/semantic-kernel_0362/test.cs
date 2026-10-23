using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Data.TextSearch;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithRegisteredIVectorSearchable_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);

            // Act
            services.AddVectorStoreTextSearch<string>();

            // Assert
            var provider = services.BuildServiceProvider();
            var registered = provider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(registered);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithNoIVectorSearchable_ShouldThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddVectorStoreTextSearch<string>();

            // Build provider
            var provider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var service = provider.GetService<VectorStoreTextSearch<string>>();
            });
            Assert.Contains("No IVectorSearchable<string> registered", exception.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithKeyedService_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<int>>();
            services.AddSingleton(mockVectorSearchable.Object);
            services.AddKeyedTransient<IVectorSearchable<int>>("testId", (sp, o) => mockVectorSearchable.Object);

            // Act
            services.AddVectorStoreTextSearch<int>("testId");

            // Assert
            var provider = services.BuildServiceProvider();
            var registered = provider.GetService<VectorStoreTextSearch<int>>();
            Assert.NotNull(registered);
        }
    }
}
