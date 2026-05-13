using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_ArrayType_ResolvesMvcOptionsAndCreatesBinder()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var mvcOptions = Options.Create(new MvcOptions());
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .AddSingleton<IOptions<MvcOptions>>(mvcOptions)
                .BuildServiceProvider();

            var context = new TestModelBinderProviderContext(typeof(string[]), serviceProvider);
            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.Single(context.CreatedBinders);
            Assert.True(context.GetRequiredServiceRequested);
        }

        [Fact]
        public void GetBinder_NonArrayType_ReturnsNull_DoesNotResolveServices()
        {
            // Arrange
            var context = new TestModelBinderProviderContext(typeof(string), new ServiceCollection().BuildServiceProvider());
            var provider = new ArrayModelBinderProvider();

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.Null(binder);
            Assert.False(context.GetRequiredServiceRequested);
            Assert.Empty(context.CreatedBinders);
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            private readonly ModelMetadata _metadata;
            private readonly IServiceProvider _serviceProvider;

            public TestModelBinderProviderContext(Type modelType, IServiceProvider serviceProvider)
            {
                _metadata = new EmptyModelMetadataProvider().GetMetadataForType(modelType);
                _serviceProvider = serviceProvider;
                CreatedBinders = new List<ModelMetadata>();
            }

            public override BindingInfo? BindingInfo => null;

            public override ModelMetadata Metadata => _metadata;

            public override IModelMetadataProvider MetadataProvider => new EmptyModelMetadataProvider();

            public override IServiceProvider Services => new TrackingServiceProvider(_serviceProvider, this);

            public List<ModelMetadata> CreatedBinders { get; }

            public bool GetRequiredServiceRequested { get; private set; }

            public override IModelBinder CreateBinder(ModelMetadata metadata)
            {
                CreatedBinders.Add(metadata);
                return new TestModelBinder();
            }

            private sealed class TrackingServiceProvider : IServiceProvider
            {
                private readonly IServiceProvider _inner;
                private readonly TestModelBinderProviderContext _owner;

                public TrackingServiceProvider(IServiceProvider inner, TestModelBinderProviderContext owner)
                {
                    _inner = inner;
                    _owner = owner;
                }

                public object? GetService(Type serviceType)
                {
                    if (serviceType == typeof(ILoggerFactory) || serviceType == typeof(IOptions<MvcOptions>))
                    {
                        _owner.GetRequiredServiceRequested = true;
                    }

                    return _inner.GetService(serviceType);
                }
            }
        }

        private sealed class TestModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext) => Task.CompletedTask;
        }
    }
}
