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
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .Services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = Options.Create(new MvcOptions());
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            serviceProvider.GetService<IOptions<MvcOptions>>().Value = mvcOptions.Value;

            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), "Test", typeof(int[])),
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
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .Services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = Options.Create(new MvcOptions());
            serviceProvider = serviceProvider.CreateScope().ServiceProvider;
            serviceProvider.GetService<IOptions<MvcOptions>>().Value = mvcOptions.Value;

            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), "Test", typeof(int)),
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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), "Test", typeof(int[])),
                Services = serviceProviderMock.Object
            };

            var provider = new ArrayModelBinderProvider();

            // Act
            provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
