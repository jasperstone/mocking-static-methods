using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Embeddings; // For IVectorSearchable
using Microsoft.SemanticKernel; // For TextSearchKernelBuilderExtensions
using Microsoft.SemanticKernel.Data; // For ITextSearchStringMapper, ITextSearchResultMapper, VectorStoreTextSearchOptions

namespace Microsoft.SemanticKernel.Tests
{
    public class TextSearchKernelBuilderExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WhenVectorSearchableNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IVectorSearchable<object>>()).Returns((IVectorSearchable<object>)null);
            servicesMock.Setup(s => s.BuildServiceProvider()).Returns(serviceProviderMock.Object);
            kernelBuilderMock.SetupGet(kb => kb.Services).Returns(servicesMock.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                kernelBuilderMock.Object.AddVectorStoreTextSearch<object>());

            Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WhenVectorSearchableRegistered_ReturnsKernelBuilder()
        {
            // Arrange
            var kernelBuilderMock = new Mock<IKernelBuilder>();
            var servicesMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vectorSearchableMock = new Mock<IVectorSearchable<object>>();
            serviceProviderMock.Setup(sp => sp.GetService<IVectorSearchable<object>>()).Returns(vectorSearchableMock.Object);
            servicesMock.Setup(s => s.BuildServiceProvider()).Returns(serviceProviderMock.Object);
            kernelBuilderMock.SetupGet(kb => kb.Services).Returns(servicesMock.Object);

            // Act
            var result = kernelBuilderMock.Object.AddVectorStoreTextSearch<object>();

            // Assert
            Assert.Same(kernelBuilderMock.Object, result);
            serviceProviderMock.Verify(sp => sp.GetService<IVectorSearchable<object>>(), Times.Once);
        }
    }
}
