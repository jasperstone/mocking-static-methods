using Microsoft.AspNetCore.Mvc;
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
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .BuildServiceProvider(),
            };

            var provider = new DictionaryModelBinderProvider();

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
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string)),
                Services = new ServiceCollection()
                    .AddLogging()
                    .AddOptions()
                    .BuildServiceProvider(),
            };

            var provider = new DictionaryModelBinderProvider();

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
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = serviceProviderMock.Object,
            };

            var provider = new DictionaryModelBinderProvider();

            // Act
            provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
