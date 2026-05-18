using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForIDictionaryType()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddScoped<ILoggerFactory, LoggerFactory>()
                .AddScoped<IOptions<MvcOptions>, OptionsWrapper<MvcOptions>>()
                .BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = new EmptyModelMetadata(typeof(Dictionary<string, int>));
            var context = new ModelBinderProviderContext(metadata, serviceProvider.GetService<ILoggerFactory>(), serviceProvider.GetService<IOptions<MvcOptions>>(), serviceProvider.GetService<IServiceScopeFactory>(), metadataProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_ForNonIDictionaryType()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .AddScoped<ILoggerFactory, LoggerFactory>()
                .AddScoped<IOptions<MvcOptions>, OptionsWrapper<MvcOptions>>()
                .BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = new EmptyModelMetadata(typeof(string));
            var context = new ModelBinderProviderContext(metadata, serviceProvider.GetService<ILoggerFactory>(), serviceProvider.GetService<IOptions<MvcOptions>>(), serviceProvider.GetService<IServiceScopeFactory>(), metadataProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
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
    }
}
