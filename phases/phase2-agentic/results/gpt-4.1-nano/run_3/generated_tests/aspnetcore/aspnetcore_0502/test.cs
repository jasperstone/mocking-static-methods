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
        public void CreateAttributeMegaRoute_CallsGetRequiredService_ForActionDescriptorCollectionProvider_And_MvcAttributeRouteHandler()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>(MockBehavior.Strict);
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();

            // Setup the IServiceProvider to return the mocked services
            mockServices.Setup(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockActionDescriptorCollectionProvider.Object);

            mockServices.Setup(sp => sp.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var route = AttributeRouting.CreateAttributeMegaRoute(mockServices.Object);

            // Assert
            Assert.NotNull(route);
            mockServices.Verify(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            mockServices.Verify(sp => sp.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }
    }
}
