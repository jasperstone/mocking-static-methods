using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test
{
    public class ControllerBaseTests
    {
        private class TestController : ControllerBase
        {
            // Expose protected members if needed, but here ControllerBase members are public
        }

        [Fact]
        public void Url_Get_ReturnsUrlHelper_FromServiceProvider()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>()))
                .Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            var controller = new TestController()
            {
                ControllerContext = controllerContext
            };

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.Same(urlHelperMock.Object, urlHelper);
            // Also verify that GetUrlHelper was called with the controller's ControllerContext
            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(controller.ControllerContext), Times.Once);
        }

        [Fact]
        public void Url_Get_ReturnsNull_WhenFactoryNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContextMock.Object
            };

            var controller = new TestController()
            {
                ControllerContext = controllerContext
            };

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.Null(urlHelper);
        }

        [Fact]
        public void Url_Set_ThrowsArgumentNullException_WhenValueIsNull()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller.Url = null!);
        }

        [Fact]
        public void Url_Set_SetsValue()
        {
            // Arrange
            var controller = new TestController();
            var urlHelperMock = new Mock<IUrlHelper>();

            // Act
            controller.Url = urlHelperMock.Object;

            // Assert
            Assert.Same(urlHelperMock.Object, controller.Url);
        }
    }
}
