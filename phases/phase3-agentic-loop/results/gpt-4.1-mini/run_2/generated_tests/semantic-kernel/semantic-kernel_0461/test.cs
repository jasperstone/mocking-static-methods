using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGeneratorMock.Object);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions
            {
                EmbeddingGenerator = embeddingGeneratorMock.Object
            };

            // Act
            var result = InvokeGetStoreOptions(serviceProvider, sp => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNullInOptionsButServiceProviderHasIt()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGeneratorMock.Object);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions
            {
                EmbeddingGenerator = null
            };

            // Act
            var result = InvokeGetStoreOptions(serviceProvider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsAndEmbeddingGeneratorAreNull()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(serviceProvider, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsIsNotNullAndEmbeddingGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var options = new MongoVectorStoreOptions
            {
                EmbeddingGenerator = null
            };

            // Act
            var result = InvokeGetStoreOptions(serviceProvider, sp => options);

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
