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
            var context = new ModelBinderProviderContext();
            context.Metadata = new DefaultModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(int[]), new PropertyName("TestProperty"), typeof(int[]));
            context.Services = new ServiceCollection()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<IOptions<MvcOptions>, OptionsWrapper<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()))
                .BuildServiceProvider();

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
            var context = new ModelBinderProviderContext();
            context.Metadata = new DefaultModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(string), new PropertyName("TestProperty"), typeof(string));
            context.Services = new ServiceCollection()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<IOptions<MvcOptions>, OptionsWrapper<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()))
                .BuildServiceProvider();

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_WhenServicesAreNull_ThrowsArgumentNullException()
        {
            // Arrange
            var context = new ModelBinderProviderContext();
            context.Metadata = new DefaultModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(int[]), new PropertyName("TestProperty"), typeof(int[]));
            context.Services = null;

            var provider = new ArrayModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(context));
        }
    }
}
