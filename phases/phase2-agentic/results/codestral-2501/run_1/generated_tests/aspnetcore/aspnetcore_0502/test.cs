using Xunit;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ThrowsArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            IServiceProvider services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(services));
        }

        [Fact]
        public void CreateAttributeMegaRoute_ReturnsAttributeRoute_WhenServicesIsNotNull()
        {
            // Arrange
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockActionDescriptorCollectionProvider.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            Assert.IsType<AttributeRoute>(result);
        }

        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredService_ForActionDescriptorCollectionProvider()
        {
            // Arrange
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockActionDescriptorCollectionProvider.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
        }

        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredService_ForMvcAttributeRouteHandler()
        {
            // Arrange
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();

            mockServiceProvider
                .Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockActionDescriptorCollectionProvider.Object);

            mockServiceProvider
                .Setup(s => s.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(s => s.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }
    }
}
