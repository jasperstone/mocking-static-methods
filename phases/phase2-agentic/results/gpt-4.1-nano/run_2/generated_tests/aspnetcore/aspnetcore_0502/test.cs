using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredService_ForActionDescriptorCollectionProvider()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            var routeHandlerMock = new Mock<MvcAttributeRouteHandler>();

            // Setup the IServiceProvider to return the mocks when requested
            servicesMock.Setup(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(actionDescriptorCollectionProviderMock.Object);
            servicesMock.Setup(sp => sp.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(routeHandlerMock.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(servicesMock.Object);

            // Assert
            Assert.IsType<AttributeRoute>(result);
            servicesMock.Verify(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            servicesMock.Verify(sp => sp.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }
    }
}
