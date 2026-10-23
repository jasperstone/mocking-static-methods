using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var optionsWithEmbedding = new PostgresVectorStoreOptions
            {
                EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object
            };
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            // Act
            var result = InvokeGetStoreOptions(sp, _ => optionsWithEmbedding);

            // Assert
            Assert.Same(optionsWithEmbedding, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var optionsWithoutEmbedding = new PostgresVectorStoreOptions();
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            // Act
            var result = InvokeGetStoreOptions(sp, _ => optionsWithoutEmbedding);

            // Assert
            Assert.Same(optionsWithoutEmbedding, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var optionsWithoutEmbedding = new PostgresVectorStoreOptions();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var services = new ServiceCollection();
            services.AddSingleton(embeddingGeneratorMock.Object);
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            // Act
            var result = InvokeGetStoreOptions(sp, _ => optionsWithoutEmbedding);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(optionsWithoutEmbedding, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderIsNull_AndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            // Act
            var result = InvokeGetStoreOptions(sp, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNull_AndEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var services = new ServiceCollection();
            services.AddSingleton(embeddingGeneratorMock.Object);
            var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            // Act
            var result = InvokeGetStoreOptions(sp, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        private static PostgresVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            var result = method.Invoke(null, new object?[] { sp, optionsProvider });
            return (PostgresVectorStoreOptions?)result;
        }
    }
}
