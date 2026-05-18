using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_UsesGetRequiredService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IUrlHelperFactory, UrlHelperFactory>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var context = new ClientModelValidationContext(actionContext, null, null);

            var remoteAttribute = new RemoteAttribute("action", "controller");

            // Act
            var url = remoteAttribute.GetUrl(context);

            // Assert
            Assert.NotNull(url);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IUrlHelperFactory, MockUrlHelperFactory>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var context = new ClientModelValidationContext(actionContext, null, null);

            var remoteAttribute = new RemoteAttribute("action", "controller");

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(context));
        }

        private class MockUrlHelperFactory : IUrlHelperFactory
        {
            public IUrlHelper GetUrlHelper(ActionContext context)
            {
                return new MockUrlHelper();
            }
        }

        private class MockUrlHelper : IUrlHelper
        {
            public string Action(UrlActionContext context) => null;

            public string RouteUrl(UrlRouteContext context) => null;

            public string Link(string routeName, object values) => null;

            public bool IsLocalUrl(string url) => false;

            public string Content(string contentPath) => null;

            public IActionResult Anchor(string anchor, object values) => null;
        }

        private class UrlHelperFactory : IUrlHelperFactory
        {
            public IUrlHelper GetUrlHelper(ActionContext context)
            {
                return new UrlHelper(context);
            }
        }
    }
}
