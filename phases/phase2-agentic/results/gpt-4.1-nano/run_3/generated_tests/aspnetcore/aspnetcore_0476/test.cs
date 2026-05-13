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
        public void GetBinder_ReturnsBinder_WhenDictionaryType()
        {
            // Arrange
            var providerMock = new Mock<IModelMetadataProvider>();
            var metadataMock = new Mock<ModelMetadata>();
            var servicesMock = new ServiceCollection().BuildServiceProvider();

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.Services).Returns(servicesMock);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);

            var modelType = typeof(IDictionary<int, string>);
            metadataMock.Setup(m => m.ModelType).Returns(modelType);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            // Setup MetadataProvider to return metadata for key and value types
            var keyMetadataMock = new Mock<ModelMetadata>();
            var valueMetadataMock = new Mock<ModelMetadata>();
            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            metadataProviderMock.Setup(mp => mp.GetMetadataForType(typeof(int))).Returns(keyMetadataMock.Object);
            metadataProviderMock.Setup(mp => mp.GetMetadataForType(typeof(string))).Returns(valueMetadataMock.Object);

            // Create a service provider with required services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            var provider = new DictionaryModelBinderProvider();

            // Inject the IServiceProvider into context.Services
            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata).Returns(metadataMock.Object);
            context.Setup(c => c.Services).Returns(serviceProvider);
            context.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns((ModelMetadata m) => new Mock<IModelBinder>().Object);
            context.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            context.Setup(c => c.Metadata).Returns(metadataMock.Object);
            context.Setup(c => c.Metadata.ModelType).Returns(modelType);

            // Call GetBinder
            var result = provider.GetBinder(context.Object);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenNotDictionaryType()
        {
            // Arrange
            var metadataMock = new Mock<ModelMetadata>();
            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.Services).Returns(new ServiceCollection().BuildServiceProvider());

            var provider = new DictionaryModelBinderProvider();

            // Setup a non-dictionary type
            var nonDictType = typeof(string);
            metadataMock.Setup(m => m.ModelType).Returns(nonDictType);

            // Act
            var result = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.Null(result);
        }
    }
}
