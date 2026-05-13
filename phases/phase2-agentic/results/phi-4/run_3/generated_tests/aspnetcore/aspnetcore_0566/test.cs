using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_ShouldGenerateUrl_WhenServiceProvided()
        {
            // Arrange
            var routeData = new Microsoft.AspNetCore.Routing.RouteValueDictionary
            {
                { "controller", "Home" },
                { "action", "Index" }
            };

            var remoteAttribute = new RemoteAttribute("Index", "Home")
            {
                RouteData = routeData
            };

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData()
            };

            var context = new ClientModelValidationContext(actionContext, new[] { remoteAttribute });

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();

            urlHelperMock
                .Setup(uh => uh.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("http://localhost/Home/Index");

            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            context.ActionContext.HttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IUrlHelperFactory>(urlHelperFactoryMock.Object)
                .BuildServiceProvider();

            // Act
            var url = remoteAttribute.GetUrl(context);

            // Assert
            Assert.Equal("http://localhost/Home/Index", url);
        }
    }
}
