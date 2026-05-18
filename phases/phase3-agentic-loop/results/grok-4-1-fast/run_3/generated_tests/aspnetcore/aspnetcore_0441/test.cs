using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ControllerBaseUrlPropertyTests
    {
        [Fact]
        public void Url_Get_ReturnsCachedInstance_WhenAlreadySet()
        {
            // Arrange
            var controller = new TestController();
            var expectedUrlHelper = Mock.Of<IUrlHelper>();
            controller.Url = expectedUrlHelper;

            // Act
            var result = controller.Url;

            // Assert
            Assert.Same(expectedUrlHelper, result);
        }

        [Fact]
        public void Url_Get_ThrowsInvalidOperationException_WhenHttpContextNull()
        {
            // Arrange
            var controller = new TestController();
            controller.ControllerContext = new ControllerContext();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => controller.Url);
            Assert.Contains("Unable to resolve service for type", exception.Message);
        }

        [Fact]
        public void Url_Get_ReturnsUrlHelperFromFactory_WhenServicesAvailable()
        {
            // Arrange
            var mockFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = Mock.Of<IUrlHelper>();
            mockFactory.Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>())).Returns(mockUrlHelper);

            var services = new ServiceCollection();
            services.AddSingleton(mockFactory.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            var controller = new TestController();
            controller.ControllerContext = controllerContext;

            // Act
            var result1 = controller.Url;
            var result2 = controller.Url;

            // Assert
            Assert.Same(mockUrlHelper, result1);
            Assert.Same(result1, result2); // Cached
            mockFactory.Verify(f => f.GetUrlHelper(controller.ControllerContext), Times.Once);
        }

        [Fact]
        public void Url_Set_ThrowsArgumentNullException_WhenValueNull()
        {
            // Arrange
            var controller = new TestController();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => controller.Url = null!);
        }
    }

    public class TestController : ControllerBase
    {
    }
}
