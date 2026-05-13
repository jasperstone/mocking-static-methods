using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ModelTypeIsArray_ReturnsArrayModelBinder()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();

            var modelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null);
            var context = new ModelBinderProviderContext(modelMetadata, serviceProvider);

            var arrayModelBinderProvider = new ArrayModelBinderProvider();

            // Act
            var binder = arrayModelBinderProvider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsNotArray_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();

            var modelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int), null);
            var context = new ModelBinderProviderContext(modelMetadata, serviceProvider);

            var arrayModelBinderProvider = new ArrayModelBinderProvider();

            // Act
            var binder = arrayModelBinderProvider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_GetRequiredServiceIsCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(mvcOptionsMock.Object);

            var modelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null);
            var context = new ModelBinderProviderContext(modelMetadata, serviceProviderMock.Object);

            var arrayModelBinderProvider = new ArrayModelBinderProvider();

            // Act
            arrayModelBinderProvider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
