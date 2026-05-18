using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    // Minimal MvcOptions class for testing purposes
    public class MvcOptions
    {
    }

    // Minimal ModelMetadata class for testing purposes
    public class ModelMetadata
    {
        public Type ModelType { get; }
        public ModelMetadata ElementMetadata { get; }

        public ModelMetadata(Type modelType, ModelMetadata elementMetadata = null)
        {
            ModelType = modelType;
            ElementMetadata = elementMetadata;
        }
    }

    // Minimal ModelBinderProviderContext class for testing purposes
    public class ModelBinderProviderContext
    {
        public ModelMetadata Metadata { get; }
        public IServiceProvider Services { get; }

        public ModelBinderProviderContext(ModelMetadata metadata, IServiceProvider services)
        {
            Metadata = metadata;
            Services = services;
        }
    }

    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_WhenModelTypeIsArray_ReturnsArrayModelBinder()
        {
            // Arrange
            var context = new Mock<ModelBinderProviderContext>(MockBehavior.Strict);
            var metadata = new Mock<ModelMetadata>(MockBehavior.Strict);
            var elementMetadata = new Mock<ModelMetadata>(MockBehavior.Strict);
            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            metadata.SetupGet(m => m.ModelType).Returns(typeof(int[]));
            metadata.SetupGet(m => m.ElementMetadata).Returns(elementMetadata.Object);
            elementMetadata.SetupGet(e => e.ModelType).Returns(typeof(int));

            var loggerFactory = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var mvcOptions = new Mock<IOptions<MvcOptions>>(MockBehavior.Strict).Object;
            var mvcOptionsValue = new MvcOptions();

            serviceProvider.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactory.Object);
            serviceProvider.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(new OptionsWrapper<MvcOptions>(mvcOptionsValue));

            context.SetupGet(c => c.Metadata).Returns(metadata.Object);
            context.SetupGet(c => c.Services).Returns(serviceProvider.Object);

            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context.Object);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
            serviceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        }
    }
}
