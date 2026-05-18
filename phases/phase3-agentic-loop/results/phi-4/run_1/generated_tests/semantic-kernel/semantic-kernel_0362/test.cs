using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data; // Assuming this is the correct namespace for IVectorSearchable
using Microsoft.SemanticKernel; // Assuming this is the correct namespace for the extension methods

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
            var vectorSearchableMock = new Mock<IVectorSearchable<object>>();

            serviceProviderMock
                .Setup(sp => sp.GetService<IVectorSearchable<object>>())
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
                .Setup(sp => sp.GetService<IVectorSearchable<object>>())
                .Returns((IVectorSearchable<object>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(serviceProviderMock.Object));
        }
    }
}
