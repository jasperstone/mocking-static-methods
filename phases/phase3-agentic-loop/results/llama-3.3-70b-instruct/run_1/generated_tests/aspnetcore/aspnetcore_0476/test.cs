using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_WhenModelTypeIsDictionary()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();

            var modelMetadata = new ModelMetadata(
                typeof(Dictionary<string, string>),
                "model",
                "Model",
                "Model description",
                false,
                false,
                null,
                new ModelBindingMessageProvider(),
                new Dictionary<string, object>());

            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetMetadataForType(typeof(string))).Returns(new ModelMetadata(
                typeof(string),
                "string",
                "String",
                "String description",
                false,
                false,
                null,
                new ModelBindingMessageProvider(),
                new Dictionary<string, object>()));

            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();

            var context = new ModelBinderProviderContext(
                modelMetadata,
                metadataProvider.Object,
                serviceScope.Object,
                serviceScopeFactory.Object,
                serviceProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotDictionary()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddOptions()
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var mvcOptions = new MvcOptions();

            var modelMetadata = new ModelMetadata(
                typeof(string),
                "model",
                "Model",
                "Model description",
                false,
                false,
                null,
                new ModelBindingMessageProvider(),
                new Dictionary<string, object>());

            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(mp => mp.GetMetadataForType(typeof(string))).Returns(new ModelMetadata(
                typeof(string),
                "string",
                "String",
                "String description",
                false,
                false,
                null,
                new ModelBindingMessageProvider(),
                new Dictionary<string, object>()));

            var serviceScope = new Mock<IServiceScope>();
            var serviceScopeFactory = new Mock<IServiceScopeFactory>();

            var context = new ModelBinderProviderContext(
                modelMetadata,
                metadataProvider.Object,
                serviceScope.Object,
                serviceScopeFactory.Object,
                serviceProvider);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }
}
