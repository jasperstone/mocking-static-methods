using System;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_UsesServiceProviderEmbeddingGenerator_WhenOptionsMissing()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var embeddingGenerator = Mock.Of<IEmbeddingGenerator>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGenerator);

            var originalOptions = new PostgresVectorStoreOptions
            {
                Schema = "custom_schema"
            };

            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => originalOptions);

            Assert.NotNull(result);
            Assert.NotSame(originalOptions, result);
            Assert.Equal(originalOptions.Schema, result!.Schema);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
            Assert.Null(originalOptions.EmbeddingGenerator);

            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            serviceProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenNoOptionsOrEmbeddingGenerator()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns((object?)null);

            var result = InvokeGetStoreOptions(serviceProviderMock.Object, optionsProvider: null);

            Assert.Null(result);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            serviceProviderMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetStoreOptions_DoesNotFetchService_WhenEmbeddingGeneratorProvided()
        {
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var embeddingGenerator = Mock.Of<IEmbeddingGenerator>();

            var options = new PostgresVectorStoreOptions
            {
                Schema = "custom_schema",
                EmbeddingGenerator = embeddingGenerator
            };

            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            Assert.Same(options, result);

            serviceProviderMock.VerifyNoOtherCalls();
        }

        private static PostgresVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider serviceProvider, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            var methodInfo = typeof(PostgresServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(methodInfo);

            return (PostgresVectorStoreOptions?)methodInfo.Invoke(null, new object?[] { serviceProvider, optionsProvider });
        }
    }
}
