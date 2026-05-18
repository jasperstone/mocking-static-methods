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
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForIDictionaryType()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(Dictionary<string, int>), null),
                MetadataProvider = new EmptyModelMetadataProvider(),
                Services = serviceProvider
            };

            // Act
            var provider = new DictionaryModelBinderProvider();
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_ForNonIDictionaryType()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(string), null),
                MetadataProvider = new EmptyModelMetadataProvider(),
                Services = serviceProvider
            };

            // Act
            var provider = new DictionaryModelBinderProvider();
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ThrowsArgumentNullException_ForNullContext()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        [Fact]
        public void GetBinder_CallsGetRequiredService_ForILoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(Dictionary<string, int>), null),
                MetadataProvider = new EmptyModelMetadataProvider(),
                Services = serviceProvider
            };

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory);

            // Act
            var provider = new DictionaryModelBinderProvider();
            provider.GetBinder(context);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        }

        [Fact]
        public void GetBinder_CallsGetRequiredService_ForIOptionsMvcOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            services.Configure<MvcOptions>(options => options = mvcOptions);
            var context = new ModelBinderProviderContext
            {
                Metadata = new ModelMetadata(new EmptyModelMetadataProvider(), typeof(Dictionary<string, int>), null),
                MetadataProvider = new EmptyModelMetadataProvider(),
                Services = serviceProvider
            };

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(Options.Create(mvcOptions));

            // Act
            var provider = new DictionaryModelBinderProvider();
            provider.GetBinder(context);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
