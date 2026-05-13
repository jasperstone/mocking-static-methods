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

            var modelType = typeof(Dictionary<int, string>);
            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(modelType);

            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            var metadata = metadataMock.Object;
            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            var mvcOptions = new MvcOptions();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            // Setup IServiceProvider to return required services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(loggerFactoryMock.Object);
            serviceCollection.AddSingleton(optionsMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(optionsMock.Object);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DictionaryModelBinder<int, string>>(result);
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
            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }
    }
}
