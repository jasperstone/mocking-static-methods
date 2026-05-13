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
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
            var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();

            mockServiceProvider
                .Setup(x => x.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(mockActionDescriptorCollectionProvider.Object);

            mockServiceProvider
                .Setup(x => x.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mockMvcAttributeRouteHandler.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AttributeRoute>(result);
        }
    }
}
