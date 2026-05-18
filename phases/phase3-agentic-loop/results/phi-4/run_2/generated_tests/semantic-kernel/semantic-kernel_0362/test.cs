using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel; // Ensure this namespace is correct for the extension method
using Microsoft.SemanticKernel.Data; // Assuming this is where IVectorSearchable is defined

namespace Microsoft.SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WhenVectorSearchableIsRegistered_ShouldReturnServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vectorSearchableMock = new Mock<Microsoft.SemanticKernel.Data.IVectorSearchable<object>>();

            serviceProviderMock
                .Setup(sp => sp.GetService<Microsoft.SemanticKernel.Data.IVectorSearchable<object>>())
                .Returns(vectorSearchableMock.Object);

            // Act
            services.AddVectorStoreTextSearch<object>(serviceProviderMock.Object);

            // Assert
            // No exception should be thrown, and the service should be added
        }

        [Fact]
        public void AddVectorStoreTextSearch_WhenVectorSearchableIsNotRegistered_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService<Microsoft.SemanticKernel.Data.IVectorSearchable<object>>())
                .Returns((Microsoft.SemanticKernel.Data.IVectorSearchable<object>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(serviceProviderMock.Object));
        }
    }
}
