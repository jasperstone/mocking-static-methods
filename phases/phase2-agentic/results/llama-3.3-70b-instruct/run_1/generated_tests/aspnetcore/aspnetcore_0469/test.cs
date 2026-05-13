using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
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
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ModelTypeIsArray_ReturnsArrayModelBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options.Value = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null),
                Services = serviceProvider
            };

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsNotArray_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options.Value = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int), null),
                Services = serviceProvider
            };

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_GetRequiredServiceIsCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();
            mvcOptionsMock.SetupGet(options => options.Value).Returns(new MvcOptions());
            services.AddSingleton<ILoggerFactory>(loggerFactoryMock.Object);
            services.AddSingleton<IOptions<MvcOptions>>(mvcOptionsMock.Object);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null),
                Services = serviceProvider
            };

            var provider = new ArrayModelBinderProvider();

            // Act
            provider.GetBinder(context);

            // Assert
            loggerFactoryMock.Verify(factory => factory.CreateLogger(It.IsAny<string>()), Times.Once);
            mvcOptionsMock.Verify(options => options.Value, Times.Once);
        }
    }
}
