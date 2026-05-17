using Xunit;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Tests
{
    public class AttributeRoutingTests
    {
        [Fact]
        public void CreateAttributeMegaRoute_ShouldReturnAttributeRoute()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            var mvcAttributeRouteHandlerMock = new Mock<MvcAttributeRouteHandler>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IActionDescriptorCollectionProvider>())
                .Returns(actionDescriptorCollectionProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<MvcAttributeRouteHandler>())
                .Returns(mvcAttributeRouteHandlerMock.Object);

            // Act
            var result = AttributeRouting.CreateAttributeMegaRoute(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AttributeRoute>(result);
        }

        [Fact]
        public void CreateAttributeMegaRoute_ShouldThrowArgumentNullException_WhenServicesIsNull()
        {
            // Arrange
            IServiceProvider services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(services));
        }
    }
}
