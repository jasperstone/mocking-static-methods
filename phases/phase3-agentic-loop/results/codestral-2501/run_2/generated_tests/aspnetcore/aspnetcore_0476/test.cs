using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WithDictionaryModelType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var modelMetadataMock = new Mock<ModelMetadata>();
            modelMetadataMock.Setup(mm => mm.ModelType).Returns(typeof(Dictionary<string, string>));
            modelMetadataProviderMock.Setup(mmp => mmp.GetMetadataForType(It.IsAny<Type>())).Returns(modelMetadataMock.Object);

            var context = new ModelBinderProviderContext(
                modelMetadataMock.Object,
                modelMetadataProviderMock.Object,
                serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_WithNonDictionaryModelType_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var modelMetadataMock = new Mock<ModelMetadata>();
            modelMetadataMock.Setup(mm => mm.ModelType).Returns(typeof(string));
            modelMetadataProviderMock.Setup(mmp => mmp.GetMetadataForType(It.IsAny<Type>())).Returns(modelMetadataMock.Object);

            var context = new ModelBinderProviderContext(
                modelMetadataMock.Object,
                modelMetadataProviderMock.Object,
                serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_WithDictionaryModelType_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var modelMetadataMock = new Mock<ModelMetadata>();
            modelMetadataMock.Setup(mm => mm.ModelType).Returns(typeof(Dictionary<string, string>));
            modelMetadataProviderMock.Setup(mmp => mmp.GetMetadataForType(It.IsAny<Type>())).Returns(modelMetadataMock.Object);

            var context = new ModelBinderProviderContext(
                modelMetadataMock.Object,
                modelMetadataProviderMock.Object,
                serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
