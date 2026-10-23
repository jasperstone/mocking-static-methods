using System;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        private class TestServiceProvider : IServiceProvider
        {
            private readonly System.Collections.Generic.Dictionary<Type, object> _services = new();

            public void AddService(Type serviceType, object implementation)
            {
                _services[serviceType] = implementation;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsProvidedWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var sp = new TestServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWhenNoEmbeddingGeneratorInOptionsAndNoService()
        {
            // Arrange
            var options = new PostgresVectorStoreOptions();
            var sp = new TestServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGeneratorWhenServiceExists()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var sp = new TestServiceProvider();
            sp.AddService(typeof(IEmbeddingGenerator), embeddingGeneratorMock.Object);
            var options = new PostgresVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNullWhenOptionsProviderIsNullAndNoService()
        {
            // Arrange
            var sp = new TestServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, null);

            // Assert
            Assert.Null(result);
        }

        private static PostgresVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            return (PostgresVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
