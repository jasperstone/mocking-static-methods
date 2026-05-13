using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Core.Data.TextSearch.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearchAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vectorSearchMock = new Mock<IVectorSearchable<string>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<string>))).Returns(vectorSearchMock.Object);

            services.AddSingleton(serviceProviderMock.Object);

            // Act
            services.AddVectorStoreTextSearch<string>();

            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenIVectorSearchableIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<string>))).Returns((IVectorSearchable<string>)null);

            services.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<string>());
        }
    }
}
