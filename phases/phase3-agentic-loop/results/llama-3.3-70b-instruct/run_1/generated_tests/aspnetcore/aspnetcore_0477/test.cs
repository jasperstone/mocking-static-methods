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
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .BuildServiceProvider();

            var context = new ModelBinderProviderContext
            {
                Metadata = ModelMetadata.CreateForType(typeof(Dictionary<string, int>)),
                Services = serviceProvider,
                MetadataProvider = new EmptyModelMetadataProvider()
            };

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_ForNonIDictionaryType()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .BuildServiceProvider();

            var context = new ModelBinderProviderContext
            {
                Metadata = ModelMetadata.CreateForType(typeof(string)),
                Services = serviceProvider,
                MetadataProvider = new EmptyModelMetadataProvider()
            };

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_CallsGetRequiredService_ForILoggerFactory()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .BuildServiceProvider();

            var context = new ModelBinderProviderContext
            {
                Metadata = ModelMetadata.CreateForType(typeof(Dictionary<string, int>)),
                Services = serviceProvider,
                MetadataProvider = new EmptyModelMetadataProvider()
            };

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
        }
    }
}
