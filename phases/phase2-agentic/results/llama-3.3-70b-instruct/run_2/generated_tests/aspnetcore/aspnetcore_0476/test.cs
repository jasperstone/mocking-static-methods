using Microsoft.AspNetCore.Mvc.ModelBinding;
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
        public void GetBinder_DictionaryType_ReturnsDictionaryModelBinder()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>))
            };

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_NonDictionaryType_ReturnsNull()
        {
            // Arrange
            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(string))
            };

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_LoggerFactoryAndMvcOptionsAreResolved()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging(logging => logging.AddConsole())
                .AddOptions()
                .Configure<MvcOptions>(options => { })
                .BuildServiceProvider();

            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = serviceProvider
            };

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }
    }
}
