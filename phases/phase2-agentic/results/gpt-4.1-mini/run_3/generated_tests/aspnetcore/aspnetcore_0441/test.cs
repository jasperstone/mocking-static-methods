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
            // Expose protected members if needed, but here ControllerBase is abstract and we test properties.
        }

        [Fact]
        public void Url_Getter_Calls_GetRequiredService_And_Uses_Factory_To_GetUrlHelper()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>()))
                .Returns(urlHelperMock.Object)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            var requestServicesMock = new Mock<IServiceProvider>();
            requestServicesMock
                .Setup(sp => sp.GetRequiredService<IUrlHelperFactory>())
                .Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(requestServicesMock.Object);

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
            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(controller.ControllerContext), Times.Once);
        }

        [Fact]
        public void Url_Getter_Returns_Set_Value_If_Already_Set()
        {
            // Arrange
            var controller = new TestController();
            var urlHelperMock = new Mock<IUrlHelper>().Object;

            controller.Url = urlHelperMock;

            // Act
            var result = controller.Url;

            // Assert
            Assert.Same(urlHelperMock, result);
        }

        [Fact]
        public void Url_Setter_Throws_If_Value_Is_Null()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller.Url = null!);
        }
    }
}
