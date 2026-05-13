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

            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int[]));
            var context = new ModelBinderProviderContext(modelMetadata)
            {
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

            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int));
            var context = new ModelBinderProviderContext(modelMetadata)
            {
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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            var services = new ServiceCollection();
            services.AddSingleton(loggerFactoryMock.Object);
            services.AddSingleton(mvcOptionsMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(int[]));
            var context = new ModelBinderProviderContext(modelMetadata)
            {
                Services = serviceProvider
            };

            var provider = new ArrayModelBinderProvider();

            // Act
            provider.GetBinder(context);

            // Assert
            loggerFactoryMock.Verify(sf => sf.GetRequiredService<ILoggerFactory>(), Times.Once);
            mvcOptionsMock.Verify(sf => sf.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
