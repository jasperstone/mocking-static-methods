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
        public void GetBinder_WhenModelTypeIsDictionary_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var context = new Mock<ModelBinderProviderContext>();
            var modelMetadata = new Mock<ModelMetadata>();
            var services = new Mock<IServiceProvider>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var mvcOptions = new Mock<IOptions<MvcOptions>>();

            var dictionaryType = typeof(Dictionary<string, int>);
            modelMetadata.Setup(m => m.ModelType).Returns(dictionaryType);
            context.Setup(c => c.Metadata).Returns(modelMetadata.Object);
            context.Setup(c => c.Services).Returns(services.Object);

            services.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory.Object);
            services.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(mvcOptions.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_WhenModelTypeIsNotDictionary_ReturnsNull()
        {
            // Arrange
            var context = new Mock<ModelBinderProviderContext>();
            var modelMetadata = new Mock<ModelMetadata>();
            var services = new Mock<IServiceProvider>();

            var nonDictionaryType = typeof(string);
            modelMetadata.Setup(m => m.ModelType).Returns(nonDictionaryType);
            context.Setup(c => c.Metadata).Returns(modelMetadata.Object);
            context.Setup(c => c.Services).Returns(services.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.Null(binder);
        }
    }
}
