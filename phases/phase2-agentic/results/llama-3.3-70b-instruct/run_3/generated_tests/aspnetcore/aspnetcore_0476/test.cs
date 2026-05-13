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

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
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
                .AddLogging()
                .AddOptions()
                .Services
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var mvcOptions = serviceProvider.GetService<IOptions<MvcOptions>>();

            var provider = new DictionaryModelBinderProvider();
            var context = new ModelBinderProviderContext
            {
                Metadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(Dictionary<string, int>)),
                Services = serviceProvider
            };

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
            Assert.Same(loggerFactory, ((DictionaryModelBinder<string, int>)binder).LoggerFactory);
            Assert.Same(mvcOptions, ((DictionaryModelBinder<string, int>)binder).MvcOptions);
        }
    }
}
