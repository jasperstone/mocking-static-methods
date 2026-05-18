using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsArray_ReturnsArrayModelBinder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockMvcOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IOptions<MvcOptions>)))
                .Returns(mockMvcOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider().GetMetadataForType(typeof(int[])),
                new ModelBindingContext
                {
                    ModelMetadata = new DefaultModelMetadataProvider().GetMetadataForType(typeof(int[])),
                    ModelState = new ModelStateDictionary(),
                    ValueProvider = new SimpleValueProvider(),
                    HttpContext = new DefaultHttpContext()
                },
                mockServiceProvider.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_WhenModelTypeIsNotArray_ReturnsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockMvcOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(mockLoggerFactory.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IOptions<MvcOptions>)))
                .Returns(mockMvcOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider().GetMetadataForType(typeof(int)),
                new ModelBindingContext
                {
                    ModelMetadata = new DefaultModelMetadataProvider().GetMetadataForType(typeof(int)),
                    ModelState = new ModelStateDictionary(),
                    ValueProvider = new SimpleValueProvider(),
                    HttpContext = new DefaultHttpContext()
                },
                mockServiceProvider.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
