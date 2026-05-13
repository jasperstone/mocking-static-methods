using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_UsesGetRequiredServiceForActionDescriptorProvider()
        {
            // Arrange
            var services = new RecordingServiceProvider();
            var actionProvider = new TestActionDescriptorCollectionProvider();
            services.AddService(typeof(IActionDescriptorCollectionProvider), actionProvider);

            // Act
            var router = AttributeRouting.CreateAttributeMegaRoute(services);

            // Assert
            Assert.NotNull(router);
            Assert.Collection(
                services.RequestedServices,
                type => Assert.Equal(typeof(IActionDescriptorCollectionProvider), type));
        }

        [Fact]
        public void CreateAttributeMegaRoute_ThrowsWhenActionDescriptorProviderMissing()
        {
            // Arrange
            var services = new RecordingServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => AttributeRouting.CreateAttributeMegaRoute(services));

            Assert.Contains(nameof(IActionDescriptorCollectionProvider), exception.Message);
            Assert.Collection(
                services.RequestedServices,
                type => Assert.Equal(typeof(IActionDescriptorCollectionProvider), type));
        }

        private sealed class RecordingServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object?> _services = new();

            public List<Type> RequestedServices { get; } = new();

            public void AddService(Type type, object? instance)
            {
                _services[type] = instance;
            }

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);
                _services.TryGetValue(serviceType, out var result);
                return result;
            }
        }

        private sealed class TestActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public ActionDescriptorCollection ActionDescriptors { get; set; } =
                new ActionDescriptorCollection(Array.Empty<ActionDescriptor>(), version: 0);
        }
    }
}
