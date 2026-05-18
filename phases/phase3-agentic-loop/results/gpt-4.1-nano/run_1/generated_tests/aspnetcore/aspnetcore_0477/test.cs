using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

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
            var metadataForTypeMock = new Mock<ModelMetadata>();
            metadataForTypeMock.Setup(m => m.ModelType).Returns(typeof(int));
            metadataProviderMock.Setup(m => m.GetMetadataForType(typeof(int)))
                .Returns(metadataForTypeMock.Object);
            var metadataForStringMock = new Mock<ModelMetadata>();
            metadataForStringMock.Setup(m => m.ModelType).Returns(typeof(string));
            metadataProviderMock.Setup(m => m.GetMetadataForType(typeof(string)))
                .Returns(metadataForStringMock.Object);

            var servicesMock = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(servicesMock.GetRequiredService<ILoggerFactory>());
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(Mock.Of<IOptions<MvcOptions>>(() => .Value == new MvcOptions()));

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProviderMock.Object);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>()))
                .Returns<ModelMetadata>(m => new Mock<IModelBinder>().Object);

            var context = contextMock.Object;

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
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

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_Throws_WhenContextIsNull()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }
    }
}
