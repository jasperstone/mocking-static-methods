using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class DictionaryModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsNull_IfContextIsNull()
        {
            var provider = new DictionaryModelBinderProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        }

        [Fact]
        public void GetBinder_ReturnsNull_IfModelTypeIsNotIDictionary()
        {
            var services = new Mock<IServiceProvider>();
            var context = CreateContext(typeof(string), services.Object);

            var provider = new DictionaryModelBinderProvider();
            var binder = provider.GetBinder(context);

            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ReturnsDictionaryModelBinder_ForIDictionary()
        {
            // Arrange
            var services = new Mock<IServiceProvider>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var mvcOptions = Options.Create(new MvcOptions());

            services.Setup(s => s.GetService(typeof(ILoggerFactory))).Returns(loggerFactory.Object);
            services.Setup(s => s.GetService(typeof(IOptions<MvcOptions>))).Returns(mvcOptions);

            var context = CreateContext(typeof(Dictionary<string, int>), services.Object);

            var provider = new DictionaryModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        }

        private static ModelBinderProviderContext CreateContext(Type modelType, IServiceProvider services)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(modelType);

            var contextMock = new Mock<ModelBinderProviderContext>();
            contextMock.Setup(c => c.Metadata).Returns(metadata);
            contextMock.Setup(c => c.MetadataProvider).Returns(metadataProvider);
            contextMock.Setup(c => c.Services).Returns(services);
            contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(new Mock<IModelBinder>().Object);

            return contextMock.Object;
        }
    }

    // Minimal stub for ModelBinderProviderContext to allow mocking
    public abstract class ModelBinderProviderContext
    {
        public abstract ModelMetadata Metadata { get; }
        public abstract IModelMetadataProvider MetadataProvider { get; }
        public abstract IServiceProvider Services { get; }
        public abstract IModelBinder CreateBinder(ModelMetadata metadata);
    }
}
