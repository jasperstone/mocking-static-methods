using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null!));
        }

        [Fact]
        public void CreateAttributeMegaRoute_Succeeds_WhenActionDescriptorCollectionProviderIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IActionDescriptorCollectionProvider>(new ActionDescriptorCollectionProviderStub());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IRouter>(result);
        }

        [Fact]
        public void CreateAttributeMegaRoute_ThrowsInvalidOperationException_WhenActionDescriptorCollectionProviderIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => AttributeRouting.CreateAttributeMegaRoute(serviceProvider));
            Assert.Contains("ActionDescriptorCollectionProvider", exception.Message);
        }

        private class ActionDescriptorCollectionProviderStub : ActionDescriptorCollectionProvider
        {
            public override ActionDescriptorCollection ActionDescriptors { get; } = new(new ActionDescriptor[0]);

            public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
        }
    }
}
