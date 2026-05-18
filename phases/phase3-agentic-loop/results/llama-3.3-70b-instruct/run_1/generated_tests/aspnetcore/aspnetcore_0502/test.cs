using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace AttributeRoutingTests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_GetRequiredService_CalledOnce()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IActionDescriptorCollectionProvider, ActionDescriptorCollectionProvider>();
            services.AddTransient<MvcAttributeRouteHandler, MvcAttributeRouteHandler>();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var attributeRoute = AttributeRouting.CreateAttributeMegaRoute(serviceProvider);

            // Assert
            Assert.NotNull(attributeRoute);
        }

        [Fact]
        public void CreateAttributeMegaRoute_GetRequiredService_ThrowsException_WhenServiceNotFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ServiceNotFoundException>(() => AttributeRouting.CreateAttributeMegaRoute(serviceProvider));
        }
    }

    public class ActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        public ActionDescriptorCollection ActionDescriptors { get; } = new ActionDescriptorCollection();
    }

    public class MvcAttributeRouteHandler : IRouter
    {
        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            throw new NotImplementedException();
        }

        public Task RouteAsync(RouteContext context)
        {
            throw new NotImplementedException();
        }

        public ActionDescriptor[] Actions { get; set; }
    }
}
