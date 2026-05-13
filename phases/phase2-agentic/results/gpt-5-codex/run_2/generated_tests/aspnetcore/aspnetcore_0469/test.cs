using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Binders
{
    public class ArrayModelBinderProviderTests
    {
        [Fact]
        public void GetBinder_RequestsMvcOptionsFromServices()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(int[]));
            var elementBinder = new StubModelBinder();

            var mvcOptions = Options.Create(new MvcOptions());
            var serviceProvider = new TrackingServiceProvider(new Dictionary<Type, object?>
            {
                [typeof(ILoggerFactory)] = NullLoggerFactory.Instance,
                [typeof(IOptions<MvcOptions>)] = mvcOptions,
            });

            var context = new TestModelBinderProviderContext(metadata, serviceProvider, elementBinder);

            // Act
            var binder = provider.GetBinder(context);

            // Assert
            Assert.NotNull(binder);
            Assert.IsType<ArrayModelBinder<int>>(binder);
            Assert.Contains(typeof(IOptions<MvcOptions>), serviceProvider.RequestedServices);
        }

        [Fact]
        public void GetBinder_ThrowsWhenMvcOptionsNotRegistered()
        {
            // Arrange
            var provider = new ArrayModelBinderProvider();
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(int[]));
            var elementBinder = new StubModelBinder();
            var serviceProvider = new TrackingServiceProvider(new Dictionary<Type, object?>
            {
                [typeof(ILoggerFactory)] = NullLoggerFactory.Instance,
            });

            var context = new TestModelBinderProviderContext(metadata, serviceProvider, elementBinder);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => provider.GetBinder(context));
            Assert.Contains(typeof(IOptions<MvcOptions>), serviceProvider.RequestedServices);
        }

        private sealed class TestModelBinderProviderContext : ModelBinderProviderContext
        {
            private readonly IModelBinder _binder;

            public TestModelBinderProviderContext(ModelMetadata metadata, IServiceProvider services, IModelBinder binder)
            {
                Metadata = metadata;
                Services = services;
                _binder = binder;
            }

            public override BindingInfo? BindingInfo => null;

            public override ModelMetadata Metadata { get; }

            public override IServiceProvider Services { get; }

            public override IModelBinder CreateBinder(ModelMetadata metadata) => _binder;
        }

        private sealed class TrackingServiceProvider : IServiceProvider
        {
            private readonly IDictionary<Type, object?> _services;

            public TrackingServiceProvider(IDictionary<Type, object?> services)
            {
                _services = services;
            }

            public List<Type> RequestedServices { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        private sealed class StubModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext) => Task.CompletedTask;
        }
    }
}
