using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ArrayType_ReturnsBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new Mock<IOptions<MvcOptions>>().Object);
            var serviceProvider = services.BuildServiceProvider();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Services).Returns(serviceProvider);

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            var elementMetadataMock = new Mock<ModelMetadata>();
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            contextMock.Verify(c => c.CreateBinder(It.IsAny<ModelMetadata>()), Times.Once);
        }

        [Fact]
        public void GetBinder_NonArrayType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new Mock<IOptions<MvcOptions>>().Object);
            var serviceProvider = services.BuildServiceProvider();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Services).Returns(serviceProvider);

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(List<int>));
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
            Assert.Equal("context", exception.ParamName);
        }
    }
}
