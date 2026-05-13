using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null!));
        }

        [Fact]
        public void CreateAttributeMegaRoute_ReturnsAttributeRoute_WithExpectedDependencies()
        {
            // Arrange
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>(MockBehavior.Strict);
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            // Setup GetRequiredService for IActionDescriptorCollectionProvider
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(IActionDescriptorCollectionProvider)))
                .Returns(mockActionDescriptorCollectionProvider.Object);

            // Setup GetRequiredService for MvcAttributeRouteHandler
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService(typeof(MvcAttributeRouteHandler)))
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var router = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(router);
            Assert.IsType<AttributeRoute>(router);

            // Verify that GetRequiredService was called for IActionDescriptorCollectionProvider
            mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(IActionDescriptorCollectionProvider)), Times.Once);

            // Verify that GetRequiredService was called for MvcAttributeRouteHandler when the handler factory is invoked
            // We invoke the handler factory by calling the internal delegate with an empty array
            var attributeRoute = (AttributeRoute)router;
            var handler = attributeRoute.GetType()
                .GetField("_handlerFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(attributeRoute) as Func<ActionDescriptor[], IRouter>;

            Assert.NotNull(handler);

            var resultHandler = handler!(Array.Empty<ActionDescriptor>());
            Assert.Same(mockMvcAttributeRouteHandler.Object, resultHandler);

            // Verify that GetRequiredService was called for MvcAttributeRouteHandler
            mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(MvcAttributeRouteHandler)), Times.Once);
        }
    }
}
