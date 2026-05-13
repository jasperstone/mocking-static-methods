using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_DictionaryType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = new ServiceCollection().BuildServiceProvider(),
            };

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string)),
                Services = new ServiceCollection().BuildServiceProvider(),
            };

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);

            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = serviceProviderMock.Object,
            };

            // Act
            provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void GetBinder_GetRequiredService_ThrowsException_WhenServiceNotFound()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Throws<InvalidOperationException>();

            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = serviceProviderMock.Object,
            };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetBinder(context));
        }
    }
}
