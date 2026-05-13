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
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_CallsGetRequiredServiceForLoggerFactory()
        {
            // Arrange
            var modelType = typeof(IDictionary<int, string>);
            var dictionaryType = typeof(IDictionary<,>);
            var genericArguments = new Type[] { typeof(int), typeof(string) };

            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(modelType);

            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var metadataForTypeMock = new Mock<ModelMetadata>();
            metadataForTypeMock.Setup(m => m.ModelType).Returns(typeof(int));
            var metadataForValueTypeMock = new Mock<ModelMetadata>();
            metadataForValueTypeMock.Setup(m => m.ModelType).Returns(typeof(string));
            metadataProviderMock.Setup(p => p.GetMetadataForType(typeof(int))).Returns(metadataForTypeMock.Object);
            metadataProviderMock.Setup(p => p.GetMetadataForType(typeof(string))).Returns(metadataForValueTypeMock.Object);

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            // Setup IServiceProvider to return the mocked ILoggerFactory and IOptions<MvcOptions>
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(optionsMock.Object);

            var serviceProvider = servicesMock.Object;

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            servicesMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        }
    }
}
