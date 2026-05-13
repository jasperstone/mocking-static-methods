using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AttributeRoutingTests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ReturnsAttributeRoute_WithCorrectServices()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();

            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            var routeHandlerMock = new Mock<MvcAttributeRouteHandler>();

            // Setup GetRequiredService to return specific mocks based on type
            servicesMock.Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(actionDescriptorCollectionProviderMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(routeHandlerMock.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(servicesMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AttributeRoute>(result);

            // Verify that GetRequiredService was called for IActionDescriptorCollectionProvider
            servicesMock.Verify(s => s.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);

            // Verify that GetRequiredService was called for MvcAttributeRouteHandler
            servicesMock.Verify(s => s.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);

            // Check that the handler's Actions property can be set
            var attributeRoute = result as AttributeRoute;
            Assert.NotNull(attributeRoute);
        }
    }
}
