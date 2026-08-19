using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearch()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<>))).Returns((object)null);

            // Act
            serviceCollection.AddVectorStoreTextSearch<TestRecord>();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<TestRecord>>();
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenIVectorSearchableIsNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<>))).Returns((object)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddVectorStoreTextSearch<TestRecord>());
        }

        private class TestRecord
        {
        }
    }
}
