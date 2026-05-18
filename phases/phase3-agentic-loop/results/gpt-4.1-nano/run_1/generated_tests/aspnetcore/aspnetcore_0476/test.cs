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
        public void GetBinder_ReturnsBinder_WhenDictionaryType()
        {
            // Arrange
            var providerMock = new Mock<IModelMetadataProvider>();
            var metadataMock = new Mock<ModelMetadata>();
            var contextMock = new Mock<ModelBinderProviderContext>();
            var services = new ServiceCollection();

            var loggerFactory = new LoggerFactory();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            var options = Options.Create(new MvcOptions());
            services.AddSingleton<IOptions<MvcOptions>>(options);

            var serviceProvider = services.BuildServiceProvider();

            var dictionaryType = typeof(IDictionary<int, string>);
            metadataMock.Setup(m => m.ModelType).Returns(dictionaryType);
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);
            contextMock.Setup(c => c.Services).Returns(serviceProvider);
            var metadataProviderMock = new Mock<IModelMetadataProvider>();
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProviderMock.Object);
            var createBinderMock = new Mock<Func<ModelMetadata, IModelBinder>>();
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns<ModelMetadata>(m => new Mock<IModelBinder>().Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(contextMock.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenNotDictionaryType()
        {
            // Arrange
            var contextMock = new Mock<ModelBinderProviderContext>();
            var metadataMock = new Mock<ModelMetadata>();
            metadataMock.Setup(m => m.ModelType).Returns(typeof(string));
            contextMock.Setup(c => c.Metadata).Returns(metadataMock.Object);

            var provider = new DictionaryModelBinderProvider();

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
