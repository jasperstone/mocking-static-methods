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
        public void CreateAttributeMegaRoute_CallsGetRequiredServiceOnServices()
        {
            // Arrange
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>(MockBehavior.Strict);
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();
            mockMvcAttributeRouteHandler.SetupAllProperties();

            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            // Setup GetRequiredService for IActionDescriptorCollectionProvider
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IActionDescriptorCollectionProvider)))
                .Returns(mockActionDescriptorCollectionProvider.Object);

            // Setup GetRequiredService for MvcAttributeRouteHandler
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(MvcAttributeRouteHandler)))
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Setup extension method GetRequiredService to call GetService and throw if null
            // We simulate the extension method by using the IServiceProvider.GetService method
            // The tested code calls services.GetRequiredService<T>(), which calls IServiceProvider.GetService(typeof(T)) internally.

            // Act
            var router = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(router);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IActionDescriptorCollectionProvider)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(MvcAttributeRouteHandler)), Times.Once);
            Assert.Same(mockMvcAttributeRouteHandler.Object, ((AttributeRoute)router).GetType()
                .GetField("_handlerFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(router)!.DynamicInvoke(Array.Empty<ActionDescriptor>()));
        }
    }
}
