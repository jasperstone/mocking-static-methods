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
            Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null!));
        }

        [Fact]
        public void CreateAttributeMegaRoute_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>(MockBehavior.Strict);
            var mvcAttributeRouteHandlerMock = new Mock<MvcAttributeRouteHandler>(MockBehavior.Strict);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);

            // Setup the IServiceProvider mock to return the mocks when GetService is called
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IActionDescriptorCollectionProvider)))
                .Returns(actionDescriptorCollectionProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(MvcAttributeRouteHandler)))
                .Returns(mvcAttributeRouteHandlerMock.Object);

            // Act
            var router = AttributeRouting.CreateAttributeMegaRoute(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(router);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActionDescriptorCollectionProvider)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(MvcAttributeRouteHandler)), Times.AtLeastOnce);
        }
    }
}
