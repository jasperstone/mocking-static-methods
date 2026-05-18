using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsBinder_ForDictionaryType()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();
            mvcOptionsMock.Setup(m => m.Value).Returns(new MvcOptions());

            servicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetService(typeof(IOptions<MvcOptions>))).Returns(mvcOptionsMock.Object);

            var metadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(modelMetadata);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProvider);
            contextMock.Setup(c => c.Services).Returns(servicesMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(new SimpleTypeModelBinder(typeof(object)));

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
        }
    }
}
