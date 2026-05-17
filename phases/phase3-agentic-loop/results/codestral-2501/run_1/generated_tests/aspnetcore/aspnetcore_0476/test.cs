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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var context = new ModelBinderProviderContext(
                modelMetadataProviderMock.Object,
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(Dictionary<string, string>), new PropertyNameProvider()),
                new ModelBindingContext
                {
                    ModelName = "test",
                    ValueProvider = new SimpleValueProvider(),
                    ModelState = new ModelStateDictionary(),
                    Services = serviceProviderMock.Object
                });

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, string>>(binder);
        }

        [Fact]
        public void GetBinder_WithNonDictionaryModelType_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var context = new ModelBinderProviderContext(
                modelMetadataProviderMock.Object,
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(string), new PropertyNameProvider()),
                new ModelBindingContext
                {
                    ModelName = "test",
                    ValueProvider = new SimpleValueProvider(),
                    ModelState = new ModelStateDictionary(),
                    Services = serviceProviderMock.Object
                });

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_WithDictionaryModelType_CallsGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<MvcOptions>>())
                .Returns(mvcOptionsMock.Object);

            var modelMetadataProviderMock = new Mock<IModelMetadataProvider>();
            var context = new ModelBinderProviderContext(
                modelMetadataProviderMock.Object,
                new ModelMetadata(new EmptyModelMetadataProvider(), new ModelAttributes(), typeof(Dictionary<string, string>), new PropertyNameProvider()),
                new ModelBindingContext
                {
                    ModelName = "test",
                    ValueProvider = new SimpleValueProvider(),
                    ModelState = new ModelStateDictionary(),
                    Services = serviceProviderMock.Object
                });

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILoggerFactory>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }

    public class EmptyModelMetadataProvider : IModelMetadataProvider
    {
        public ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            return new ModelMetadata(this, new ModelAttributes(), typeof(object), new PropertyNameProvider());
        }

        public ModelMetadata GetMetadataForType(Type modelType)
        {
            return new ModelMetadata(this, new ModelAttributes(), modelType, new PropertyNameProvider());
        }

        public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
        {
            return Array.Empty<ModelMetadata>();
        }
    }

    public class SimpleValueProvider : IValueProvider
    {
        public bool ContainsPrefix(string prefix)
        {
            return false;
        }

        public ValueProviderResult GetValue(string key)
        {
            return ValueProviderResult.None;
        }
    }
}
