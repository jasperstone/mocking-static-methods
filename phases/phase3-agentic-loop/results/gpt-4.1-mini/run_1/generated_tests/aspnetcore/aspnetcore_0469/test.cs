using System;
using Microsoft.AspNetCore.Mvc;
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
        private class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public TestModelBinderProviderContext(ModelMetadata metadata, IServiceProvider services)
            {
                Metadata = metadata;
                Services = services;
            }

            public override ModelMetadata Metadata { get; }

            public override IServiceProvider Services { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                // Return a simple mock binder for element binder
                return new Mock<IModelBinder>().Object;
            }
        }

        [Fact]
        public void GetBinder_ReturnsNull_IfModelTypeIsNotArray()
        {
            // Arrange
            var metadataMock = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(typeof(string)));
            metadataMock.Setup(m => m.ModelType).Returns(typeof(string));
            var servicesMock = new Mock<IServiceProvider>();

            var context = new TestModelBinderProviderContext(metadataMock.Object, servicesMock.Object);
            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ReturnsArrayModelBinder_AndCallsGetRequiredService()
        {
            // Arrange
            var elementType = typeof(int);
            var arrayType = elementType.MakeArrayType();

            var elementMetadataMock = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(elementType));
            elementMetadataMock.Setup(m => m.ModelType).Returns(elementType);

            var metadataMock = new Mock<ModelMetadata>(ModelMetadataIdentity.ForType(arrayType));
            metadataMock.Setup(m => m.ModelType).Returns(arrayType);
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            var context = new TestModelBinderProviderContext(metadataMock.Object, servicesMock.Object);
            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.Equal(typeof(ArrayModelBinder<int>), binder.GetType());

            servicesMock.Verify(s => s.GetService(typeof(ILoggerFactory)), Times.Once);
            servicesMock.Verify(s => s.GetService(typeof(IOptions<MvcOptions>)), Times.Once);
        }
    }
}
