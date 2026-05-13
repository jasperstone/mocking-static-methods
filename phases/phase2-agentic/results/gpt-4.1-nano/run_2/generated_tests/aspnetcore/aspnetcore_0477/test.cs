using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsBinder_WhenModelTypeIsDictionary()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            var modelType = typeof(IDictionary<int, string>);
            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(modelType);

            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var metadata = metadataMock.Object;
            metadataProviderMock.Setup(m => m.GetMetadataForType(It.IsAny<Type>())).Returns(metadata);

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);
            serviceCollection.AddSingleton(optionsMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var context = contextMock.Object;

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotDictionary()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            var modelType = typeof(string);
            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(modelType);

            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var metadata = metadataMock.Object;

            var serviceProviderMock = new Mock<IServiceProvider>();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProviderMock.Object);

            var context = contextMock.Object;

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_Throws_When_GetRequiredServiceFails()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            var modelType = typeof(IDictionary<int, string>);
            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(modelType);

            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var metadata = metadataMock.Object;

            var serviceCollection = new ServiceCollection();
            // Do not add ILoggerFactory or IOptions<MvcOptions> to simulate failure
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);

            var context = contextMock.Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetBinder(context));
        }
    }
}
