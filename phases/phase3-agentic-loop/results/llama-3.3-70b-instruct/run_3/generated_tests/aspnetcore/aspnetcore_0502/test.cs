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
            services.AddTransient<IActionDescriptorCollectionProvider, MockActionDescriptorCollectionProvider>();
            services.AddTransient<MvcAttributeRouteHandler, MockMvcAttributeRouteHandler>();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var attributeRoute = AttributeRouting.CreateAttributeMegaRoute(serviceProvider);

            // Assert
            Assert.NotNull(attributeRoute);
        }

        [Fact]
        public void CreateAttributeMegaRoute_GetRequiredService_ThrowsException_WhenServiceProviderIsNull()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null));
        }
    }

    public class MockActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
    {
        public ActionDescriptorCollection ActionDescriptors { get; } = new ActionDescriptorCollection();
    }

    public class MockMvcAttributeRouteHandler : IRouter
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
