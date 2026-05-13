using System;
using System.Reflection;
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
        private class DummyModelBinder : IModelBinder
        {
            public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GetBinder_ReturnsNull_IfModelTypeIsNotArray()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var metadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, ModelMetadataIdentity.ForType(typeof(string)));
            metadataMock.Setup(m => m.ModelType).Returns(typeof(string));

            var contextMock = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_ReturnsArrayModelBinder_ForArrayModelType()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var elementType = typeof(int);

            var elementMetadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, ModelMetadataIdentity.ForType(elementType));
            elementMetadataMock.Setup(m => m.ModelType).Returns(elementType);

            var arrayType = elementType.MakeArrayType();

            var metadataMock = new Mock<ModelMetadata>(MockBehavior.Strict, ModelMetadataIdentity.ForType(arrayType));
            metadataMock.Setup(m => m.ModelType).Returns(arrayType);
            metadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

            var elementBinder = new DummyModelBinder();

            var servicesMock = new Mock<IServiceProvider>(MockBehavior.Strict);

            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            servicesMock.Setup(s => s.GetRequiredService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);

            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>(MockBehavior.Strict);
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);
            servicesMock.Setup(s => s.GetRequiredService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            var contextMock = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.CreateBinder(elementMetadataMock.Object)).Returns(elementBinder);
            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            var binderType = typeof(ArrayModelBinder<>).MakeGenericType(elementType);
            Assert.IsType(binderType, binder);
        }
    }
}
