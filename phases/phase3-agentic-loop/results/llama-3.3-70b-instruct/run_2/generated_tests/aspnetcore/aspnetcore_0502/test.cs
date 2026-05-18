using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;

namespace AttributeRoutingTests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_GetRequiredService_CalledOnce()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IActionDescriptorCollectionProvider>()).Returns(actionDescriptorCollectionProviderMock.Object);
            var mvcAttributeRouteHandlerMock = new Mock<MvcAttributeRouteHandler>();
            servicesMock.Setup(s => s.GetRequiredService<MvcAttributeRouteHandler>()).Returns(mvcAttributeRouteHandlerMock.Object);

            // Act
            AttributeRouting.CreateAttributeMegaRoute(servicesMock.Object);

            // Assert
            servicesMock.Verify(s => s.GetRequiredService<IActionDescriptorCollectionProvider>(), Times.Once);
            servicesMock.Verify(s => s.GetRequiredService<MvcAttributeRouteHandler>(), Times.Once);
        }

        [Fact]
        public void CreateAttributeMegaRoute_GetRequiredService_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null));
        }
    }
}
