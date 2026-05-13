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
            var metadata = new Mock<ModelMetadata>();
            metadata.Setup(m => m.ModelType).Returns(modelType);

            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(m => m.GetMetadataForType(It.IsAny<Type>()))
                .Returns<Type>(t => new Mock<ModelMetadata>().Object);

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(Mock.Of<ILoggerFactory>());
            servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(Mock.Of<IOptions<MvcOptions>>(() => .Value == new MvcOptions()));

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata).Returns(metadata.Object);
            context.Setup(c => c.MetadataProvider).Returns(metadataProvider.Object);
            context.Setup(c => c.Services).Returns(serviceProvider);

            // Act
            var binder = provider.GetBinder(context.Object);

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
            var metadata = new Mock<ModelMetadata>();
            metadata.Setup(m => m.ModelType).Returns(modelType);

            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(m => m.GetMetadataForType(It.IsAny<Type>()))
                .Returns<Type>(t => new Mock<ModelMetadata>().Object);

            var serviceCollection = new ServiceCollection();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata).Returns(metadata.Object);
            context.Setup(c => c.MetadataProvider).Returns(metadataProvider.Object);
            context.Setup(c => c.Services).Returns(serviceProvider);

            // Act
            var result = provider.GetBinder(context.Object);

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
