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
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ArrayModelType_ReturnsArrayModelBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions<MvcOptions>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();
            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null);
            var context = new ModelBinderProviderContext(metadata, serviceProvider);

            // Act
            var provider = new ArrayModelBinderProvider();
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_NonArrayModelType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions<MvcOptions>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();
            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int), null);
            var context = new ModelBinderProviderContext(metadata, serviceProvider);

            // Act
            var provider = new ArrayModelBinderProvider();
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions<MvcOptions>();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();
            var metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(int[]), null);
            var context = new ModelBinderProviderContext(metadata, serviceProvider);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory);
            serviceProviderMock.Setup(p => p.GetRequiredService<IOptions<MvcOptions>>()).Returns(mvcOptions);

            // Act
            var provider = new ArrayModelBinderProvider();
            var binder = provider.GetBinder(new ModelBinderProviderContext(metadata, serviceProviderMock.Object));

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(p => p.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
