using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ControllerBaseTests
    {
        [Fact]
        public void Url_ShouldUseUrlHelperFactoryFromRequestServices()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>()))
                .Returns(urlHelperMock.Object);

            httpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IUrlHelperFactory>(urlHelperFactoryMock.Object)
                .BuildServiceProvider();

            var controllerBase = new TestControllerBase
            {
                ControllerContext = controllerContext
            };

            // Act
            var url = controllerBase.Url;

            // Assert
            Assert.Same(urlHelperMock.Object, url);
            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(controllerContext), Times.Once);
        }

        private class TestControllerBase : ControllerBase
        {
        }
    }
}
