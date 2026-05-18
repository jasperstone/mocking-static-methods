using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotDictionary()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(string), null);
            var metadataProvider = new EmptyModelMetadataProvider();
            var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
            var context = new ModelBinderProviderContext(metadata, metadataProvider, services);

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_WhenModelTypeIsDictionary()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(Dictionary<string, int>), null);
            var metadataProvider = new EmptyModelMetadataProvider();
            var services = new ServiceCollection().AddLogging().AddOptions().BuildServiceProvider();
            var context = new ModelBinderProviderContext(metadata, metadataProvider, services);

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        [Fact]
        public void GetBinder_GetRequiredService_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(mvcOptionsMock.Object);

            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(Dictionary<string, int>), null);
            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext(metadata, metadataProvider, serviceProviderMock.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
