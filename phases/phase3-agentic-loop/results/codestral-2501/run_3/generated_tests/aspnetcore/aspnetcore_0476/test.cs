using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WithDictionaryType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new DefaultModelBinderFactory(),
                mockServiceProvider.Object,
                new ModelBindingInfo(),
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(Dictionary<string, string>), new PropertyNameProvider()));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_WithNonDictionaryType_ReturnsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new DefaultModelBinderFactory(),
                mockServiceProvider.Object,
                new ModelBindingInfo(),
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(string), new PropertyNameProvider()));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
