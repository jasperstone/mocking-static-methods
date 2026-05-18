using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        private class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            public override ModelMetadata Metadata { get; }
            public override IModelMetadataProvider MetadataProvider { get; }
            public override IServiceProvider Services { get; }
            private readonly Func<ModelMetadata, IModelBinder> _createBinder;

            public TestModelBinderProviderContext(
                ModelMetadata metadata,
                IModelMetadataProvider metadataProvider,
                IServiceProvider services,
                Func<ModelMetadata, IModelBinder> createBinder)
            {
                Metadata = metadata;
                MetadataProvider = metadataProvider;
                Services = services;
                _createBinder = createBinder;
            }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                return _createBinder(metadata);
            }
        }

        [Fact]
        public void GetBinder_ReturnsBinder_ForDictionaryType()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var mvcOptions = new MvcOptions();
            var optionsMock = new Mock<IOptions<MvcOptions>>();
            optionsMock.Setup(o => o.Value).Returns(mvcOptions);

            servicesMock.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetService(typeof(IOptions<MvcOptions>))).Returns(optionsMock.Object);

            var metadataProvider = new EmptyModelMetadataProvider();
            var dictionaryMetadata = metadataProvider.GetMetadataForType(typeof(Dictionary<string, int>));
            var keyMetadata = metadataProvider.GetMetadataForType(typeof(string));
            var valueMetadata = metadataProvider.GetMetadataForType(typeof(int));

            var context = new TestModelBinderProviderContext(
                dictionaryMetadata,
                metadataProvider,
                servicesMock.Object,
                metadata =>
                {
                    if (metadata.ModelType == typeof(string))
                    {
                        return new SimpleTypeModelBinder(typeof(string));
                    }
                    if (metadata.ModelType == typeof(int))
                    {
                        return new SimpleTypeModelBinder(typeof(int));
                    }
                    return null!;
                });

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_ForNonDictionaryType()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();

            var metadataProvider = new EmptyModelMetadataProvider();
            var stringMetadata = metadataProvider.GetMetadataForType(typeof(string));

            var context = new TestModelBinderProviderContext(
                stringMetadata,
                metadataProvider,
                servicesMock.Object,
                metadata => null!);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
        }
    }

    // Abstract class stub for ModelBinderProviderContext to allow testing
    public abstract class ModelBinderProviderContext
    {
        public abstract ModelMetadata Metadata { get; }
        public abstract IModelMetadataProvider MetadataProvider { get; }
        public abstract IServiceProvider Services { get; }
        public abstract IModelBinder CreateBinder(ModelMetadata metadata);
    }
}
