using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        private class DummyEmbeddingGenerator : IEmbeddingGenerator { }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions
            {
                EmbeddingGenerator = new DummyEmbeddingGenerator()
            };

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsHasNoEmbeddingGeneratorAndServiceProviderHas()
        {
            // Arrange
            var embeddingGenerator = new DummyEmbeddingGenerator();
            var services = new ServiceCollection();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsIsNullAndServiceProviderHasNoEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasNoEmbeddingGeneratorAndServiceProviderHasNoEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        private static MongoVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (MongoVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
