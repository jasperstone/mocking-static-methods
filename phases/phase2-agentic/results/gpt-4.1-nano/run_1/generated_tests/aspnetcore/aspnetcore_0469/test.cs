using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsArrayModelBinder_WhenModelTypeIsArray()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var elementMetadataMock = new Mock<ModelMetadata>();
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>()))
                .Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(optionsMock.Object);

            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotArray()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(string));

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        [Fact]
        public void GetBinder_CallsGetRequiredService_ForLoggerFactoryAndMvcOptions()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var elementMetadataMock = new Mock<ModelMetadata>();
            elementMetadataMock.Setup(m => m.ModelType).Returns(typeof(int));

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(int[]));
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>()))
                .Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var getRequiredServiceCalls = 0;
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(() =>
                {
                    getRequiredServiceCalls++;
                    return loggerFactoryMock.Object;
                });
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(() =>
                {
                    getRequiredServiceCalls++;
                    return optionsMock.Object;
                });

            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
            Assert.Equal(2, getRequiredServiceCalls);
        }
    }
}
