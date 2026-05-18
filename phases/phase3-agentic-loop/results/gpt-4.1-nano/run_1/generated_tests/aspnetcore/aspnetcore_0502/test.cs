using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredServiceForCorrectTypes()
        {
            // Arrange
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockHandler = new Mock<MvcAttributeRouteHandler>();
            var servicesMock = new Mock<IServiceProvider>();

            // Setup the IServiceProvider to return the mocks
            servicesMock.Setup(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockDescriptorProvider.Object);
            servicesMock.Setup(sp => sp.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockHandler.Object);

            // Act
            var route = AttributeRouting.CreateAttributeMegaRoute(servicesMock.Object);

            // Assert
            Assert.NotNull(route);
            Assert.IsType<AttributeRoute>(route);
            // Verify that GetRequiredService was called for both types
            servicesMock.Verify(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            servicesMock.Verify(sp => sp.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }
    }
}
