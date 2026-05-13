using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsDictionary_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProviderMock = new Mock<ModelMetadataProvider>();
            var modelMetadata = new Mock<ModelMetadata>(modelMetadataProviderMock.Object, new DefaultCompositeMetadataDetailsProvider(), new DefaultMetadataDetailsProvider(), new DefaultModelBindingMessageProvider(), new DefaultModelValidatorProvider(), new DefaultMetadataDetailsProvider(), new DefaultModelMetadataDetailsProvider());
            modelMetadata.Setup(mm => mm.ModelType).Returns(typeof(Dictionary<string, string>));

            var context = new ModelBinderProviderContext(
                modelMetadata.Object,
                new DefaultModelMetadataProvider(),
                serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_WhenModelTypeIsNotDictionary_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(Mock.Of<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProviderMock = new Mock<ModelMetadataProvider>();
            var modelMetadata = new Mock<ModelMetadata>(modelMetadataProviderMock.Object, new DefaultCompositeMetadataDetailsProvider(), new DefaultMetadataDetailsProvider(), new DefaultModelBindingMessageProvider(), new DefaultModelValidatorProvider(), new DefaultMetadataDetailsProvider(), new DefaultModelMetadataDetailsProvider());
            modelMetadata.Setup(mm => mm.ModelType).Returns(typeof(string));

            var context = new ModelBinderProviderContext(
                modelMetadata.Object,
                new DefaultModelMetadataProvider(),
                serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
