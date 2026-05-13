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
            var mockServices = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServices.Setup(s => s.GetRequiredService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockServices.Setup(s => s.GetRequiredService(typeof(IOptions<MvcOptions>))).Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider().GetMetadataForType(typeof(int[])),
                new ModelBindingContext
                {
                    ModelName = "test",
                    ModelState = new ModelStateDictionary(),
                    ValueProvider = new SimpleValueProvider(),
                    Services = mockServices.Object
                });

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
            var mockServices = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServices.Setup(s => s.GetRequiredService(typeof(ILoggerFactory))).Returns(mockLoggerFactory.Object);
            mockServices.Setup(s => s.GetRequiredService(typeof(IOptions<MvcOptions>))).Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider().GetMetadataForType(typeof(int)),
                new ModelBindingContext
                {
                    ModelName = "test",
                    ModelState = new ModelStateDictionary(),
                    ValueProvider = new SimpleValueProvider(),
                    Services = mockServices.Object
                });

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
