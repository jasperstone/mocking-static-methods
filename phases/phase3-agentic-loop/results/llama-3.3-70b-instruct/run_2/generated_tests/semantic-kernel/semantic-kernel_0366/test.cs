using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new Mock<VectorStoreTextSearchOptions>();
            services.AddSingleton(stringMapperMock.Object);
            services.AddSingleton(resultMapperMock.Object);
            services.AddSingleton(optionsMock.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearchServiceId", stringMapperMock.Object, resultMapperMock.Object, optionsMock.Object);

            // Assert
            Assert.NotNull(serviceProvider.GetService<ITextSearchStringMapper>());
            Assert.NotNull(serviceProvider.GetService<ITextSearchResultMapper>());
        }

        [Fact]
        public void AddVectorStoreTextSearch_GetServiceCalled_WithNullMappers()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearchServiceId");

            // Assert
            Assert.NotNull(serviceProvider.GetService<ITextSearchStringMapper>());
            Assert.NotNull(serviceProvider.GetService<ITextSearchResultMapper>());
        }
    }
}
