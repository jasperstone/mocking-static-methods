using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton<IOptions<MvcOptions>>(Options.Create(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            var elementMetadataMock = new Mock<ModelMetadata>();
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(Mock.Of<IModelBinder>());

            var provider = new ArrayModelBinderProvider();

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(result);
            contextMock.Verify(c => c.CreateBinder(It.IsAny<ModelMetadata>()), Times.Once);
        }

        [Fact]
        public void GetBinder_NonArrayType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton<IOptions<MvcOptions>>(Options.Create(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(string));

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }
    }
}
