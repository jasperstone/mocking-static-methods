using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ControllerBaseTests
    {
        [Fact]
        public void Url_ShouldUseGetRequiredServiceToGetUrlHelperFactory()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var requestServices = new Mock<IServiceProvider>();
            var urlHelperFactory = new Mock<IUrlHelperFactory>();
            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            requestServices
                .Setup(s => s.GetRequiredService<IUrlHelperFactory>())
                .Returns(urlHelperFactory.Object);

            httpContext.RequestServices = requestServices.Object;

            var controllerBase = new Mock<ControllerBase>();
            controllerBase.Object.ControllerContext = controllerContext;

            // Act
            var url = controllerBase.Object.Url;

            // Assert
            urlHelperFactory.Verify(f => f.GetUrlHelper(controllerContext), Times.Once);
            Assert.NotNull(url);
        }
    }
}
