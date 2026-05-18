using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ReturnsRouterAndCallsGetRequiredService()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockDescriptorProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockHandler = new Mock<MvcAttributeRouteHandler>();

            // Setup GetRequiredService to return specific instances based on the type
            mockServices.Setup(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockDescriptorProvider.Object);
            mockServices.Setup(sp => sp.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockHandler.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(mockServices.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AttributeRoute>(result);

            // Verify that GetRequiredService was called with the expected types
            mockServices.Verify(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            mockServices.Verify(sp => sp.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }
    }
}
