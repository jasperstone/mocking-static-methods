using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
            services.AddOptions<MvcOptions>()
                .Configure(o => { });
            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext(metadataProvider)
            {
                Services = serviceProvider
            };
            context.MetadataProvider = metadataProvider;
            context.Metadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetBinder_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<ILoggerFactory>(new NullLoggerFactory());
            services.AddOptions<MvcOptions>()
                .Configure(o => { });
            var serviceProvider = services.BuildServiceProvider();

            var metadataProvider = new EmptyModelMetadataProvider();
            var context = new ModelBinderProviderContext(metadataProvider)
            {
                Services = serviceProvider,
                MetadataProvider = metadataProvider
            };
            context.Metadata = metadataProvider.GetMetadataForType(typeof(string));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }
    }
}
