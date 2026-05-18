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
        public void GetBinder_WhenModelTypeIsDictionary_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockOptions = new Mock<IOptions<MvcOptions>>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<ILoggerFactory>()).Returns(mockLoggerFactory.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>()).Returns(mockOptions.Object);

            var context = new ModelBinderProviderContext(
                new DefaultModelMetadataProvider(),
                new ModelBindingContext
                {
                    ModelMetadata = new DefaultModelMetadata(new DefaultCompositeMetadataDetailsProvider(), new DefaultModelMetadataProvider()),
                    ModelName = "test",
                    ModelType = typeof(Dictionary<string, string>),
                    ValueProvider = new SimpleValueProvider(),
                    Services = mockServiceProvider.Object
                });

            // Act
            var provider = new DictionaryModelBinderProvider();
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }
    }

    public class SimpleValueProvider : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return true;
        }

        public ValueProviderResult GetValue(string key)
        {
            return ValueProviderResult.None;
        }
    }
}
