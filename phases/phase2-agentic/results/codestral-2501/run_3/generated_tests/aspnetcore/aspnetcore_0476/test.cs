using System;
using System.Collections.Generic;
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
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WithDictionaryModelType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.Configure<MvcOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            var modelMetadataProvider = new DefaultModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var context = new ModelBinderProviderContext(
                modelMetadata,
                new DefaultModelBinderProvider(),
                serviceProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_WithNonDictionaryModelType_ReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddOptions();
            services.Configure<MvcOptions>(options => { });
            var serviceProvider = services.BuildServiceProvider();

            var modelMetadataProvider = new DefaultModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(string));

            var context = new ModelBinderProviderContext(
                modelMetadata,
                new DefaultModelBinderProvider(),
                serviceProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_WithNullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        [Fact]
        public void GetBinder_CallsGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(x => x.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(Mock.Of<ILoggerFactory>());

            mockServiceProvider
                .Setup(x => x.GetRequiredService(typeof(IOptions<MvcOptions>)))
                .Returns(Mock.Of<IOptions<MvcOptions>>());

            var modelMetadataProvider = new DefaultModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));

            var context = new ModelBinderProviderContext(
                modelMetadata,
                new DefaultModelBinderProvider(),
                mockServiceProvider.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            mockServiceProvider.Verify(x => x.GetRequiredService(typeof(ILoggerFactory)), Times.Once);
            mockServiceProvider.Verify(x => x.GetRequiredService(typeof(IOptions<MvcOptions>)), Times.Once);
        }
    }
}
