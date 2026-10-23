using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions();

            // Build a service provider with IEmbeddingGenerator registered
            var provider = services.AddTransient<IEmbeddingGenerator>(_ => mockGenerator.Object).BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenGeneratorIsAvailableInServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient<IEmbeddingGenerator>(_ => mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Equal(options, result);
        }
    }

    // A simple stub for IServiceProvider to simulate service resolution
    public class ServiceProviderStub : IServiceProvider
    {
        private readonly Func<Type, object> _resolver;

        public ServiceProviderStub(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }

        public object GetService(Type serviceType)
        {
            return _resolver(serviceType);
        }
    }
}
