using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders.Tests
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_ReturnsBinder_WhenModelTypeIsArray()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var elementMetadata = new ModelMetadataTestProvider().GetMetadataForType(typeof(int));
            var metadata = new ModelMetadataTestProvider().GetMetadataForType(typeof(int[]));
            metadata.ElementMetadata = elementMetadata;

            var services = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var options = new MvcOptions();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(sp => new LoggerFactory())
                .AddSingleton<IOptions<MvcOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var context = new ModelBinderProviderContextStub(metadata, serviceProvider);

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
        }

        [Fact]
        public void GetBinder_ReturnsNull_WhenModelTypeIsNotArray()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            var metadata = new ModelMetadataTestProvider().GetMetadataForType(typeof(int));
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();

            var context = new ModelBinderProviderContextStub(metadata, serviceProvider);

            // Act
            var result = provider.GetBinder(context);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetBinder_Throws_WhenContextIsNull()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null));
        }

        // Helper classes for testing
        private class ModelBinderProviderContextStub : ModelBinderProviderContext
        {
            private readonly ModelMetadata _metadata;
            private readonly IServiceProvider _serviceProvider;

            public ModelBinderProviderContextStub(ModelMetadata metadata, IServiceProvider serviceProvider)
            {
                _metadata = metadata;
                _serviceProvider = serviceProvider;
            }

            public override BindingInfo? BindingInfo => null;

            public override ModelMetadata Metadata => _metadata;

            public override IServiceProvider Services => _serviceProvider;

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                return new SimpleModelBinder();
            }
        }

        private class SimpleModelBinder : IModelBinder
        {
            public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
            {
                throw new NotImplementedException();
            }
        }

        private class ModelMetadataTestProvider
        {
            public ModelMetadata GetMetadataForType(Type type)
            {
                var provider = new EmptyModelMetadataProvider();
                return provider.GetMetadataForType(type);
            }
        }
    }
}
