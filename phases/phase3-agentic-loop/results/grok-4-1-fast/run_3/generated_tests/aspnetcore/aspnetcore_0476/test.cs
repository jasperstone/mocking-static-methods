using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_DictionaryType_ReturnsBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext
            {
                MetadataProvider = metadataProvider,
                Services = serviceProvider
            };
            context.Metadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
        }

        [Fact]
        public void GetBinder_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext
            {
                MetadataProvider = metadataProvider,
                Services = serviceProvider
            };
            context.Metadata = metadataProvider.GetMetadataForType(typeof(string));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_NullContext_ThrowsArgumentNullException()
        {
            var provider = new DictionaryModelBinderProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }

        [Fact]
        public void GetBinder_MissingILoggerFactory_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<MvcOptions>>(new OptionsWrapper<MvcOptions>(new MvcOptions()));
            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext
            {
                MetadataProvider = metadataProvider,
                Services = serviceProvider
            };
            context.Metadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var provider = new DictionaryModelBinderProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetBinder(context));
            Assert.Contains("ILoggerFactory", exception.Message);
        }
    }
}
