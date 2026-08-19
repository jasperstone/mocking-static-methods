using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Microsoft.SemanticKernel.Data.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ITextSearchStringMapper)))
                .Returns(stringMapperMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ITextSearchResultMapper)))
                .Returns(resultMapperMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)))
                .Returns(options);

            // Act
            services.AddVectorStoreTextSearch<object>(
                stringMapperMock.Object,
                resultMapperMock.Object,
                options);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.Never);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.Never);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)), Times.Never);
        }
    }
}
