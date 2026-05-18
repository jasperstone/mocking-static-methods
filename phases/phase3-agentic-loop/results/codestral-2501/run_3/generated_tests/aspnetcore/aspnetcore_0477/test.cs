using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WithDictionaryModelType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<IOptions<MvcOptions>, Mock<IOptions<MvcOptions>>>();
            var serviceProvider = services.BuildServiceProvider();

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Services).Returns(serviceProvider);
            context.Setup(c => c.Metadata.ModelType).Returns(typeof(Dictionary<string, int>));
            context.Setup(c => c.MetadataProvider).Returns(new DefaultModelMetadataProvider());

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_WithNonDictionaryModelType_ReturnsNull()
        {
            // Arrange
            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Metadata.ModelType).Returns(typeof(string));

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

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
            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton<IOptions<MvcOptions>, Mock<IOptions<MvcOptions>>>();
            var serviceProvider = services.BuildServiceProvider();

            var context = new Mock<ModelBinderProviderContext>();
            context.Setup(c => c.Services).Returns(serviceProvider);
            context.Setup(c => c.Metadata.ModelType).Returns(typeof(Dictionary<string, int>));
            context.Setup(c => c.MetadataProvider).Returns(new DefaultModelMetadataProvider());

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            context.Verify(c => c.Services.GetRequiredService<ILoggerFactory>(), Times.Once);
            context.Verify(c => c.Services.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
