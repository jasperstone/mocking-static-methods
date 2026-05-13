using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsArray_ReturnsArrayModelBinder()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(x => x.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(int[]), new PropertyName("Test")),
                new Dictionary<string, object>(),
                serviceProviderMock.Object);

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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(x => x.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(x => x.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(int), new PropertyName("Test")),
                new Dictionary<string, object>(),
                serviceProviderMock.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
