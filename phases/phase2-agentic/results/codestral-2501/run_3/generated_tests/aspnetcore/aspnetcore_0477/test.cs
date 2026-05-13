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
        public void GetBinder_WhenModelTypeIsDictionary_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new ModelBindingContext(),
                mockServiceProvider.Object,
                new Dictionary<string, object>());

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_WhenModelTypeIsNotDictionary_ReturnsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new ModelBindingContext(),
                mockServiceProvider.Object,
                new Dictionary<string, object>());

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
