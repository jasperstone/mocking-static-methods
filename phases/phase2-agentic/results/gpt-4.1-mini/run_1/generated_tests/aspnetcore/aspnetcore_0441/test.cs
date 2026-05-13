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
            // Expose protected members if needed, but here ControllerBase is abstract and we only test Url property.
        }

        [Fact]
        public void Url_Getter_Calls_GetRequiredService_And_Uses_Factory_To_GetUrlHelper()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>()))
                .Returns(urlHelperMock.Object)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            // We need to mock the extension method GetRequiredService<T>() which calls IServiceProvider.GetService and throws if null.
            // Since GetRequiredService is an extension method, it calls IServiceProvider.GetService internally.
            // So we simulate that by returning the factory from GetService.

            var httpContextMock = new Mock<HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            };

            var controller = new TestController
            {
                ControllerContext = controllerContext
            };

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.Same(urlHelperMock.Object, urlHelper);
            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(controller.ControllerContext), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IUrlHelperFactory)), Times.Once);
        }

        [Fact]
        public void Url_Getter_Returns_Set_Value_If_Already_Set()
        {
            // Arrange
            var controller = new TestController();
            var urlHelperMock = new Mock<IUrlHelper>();

            controller.Url = urlHelperMock.Object;

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.Same(urlHelperMock.Object, urlHelper);
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
