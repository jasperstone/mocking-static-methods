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
                .AddScoped<IUrlHelperFactory, MockUrlHelperFactory>()
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

        private class MockUrlHelperFactory : IUrlHelperFactory
        {
            public IUrlHelper GetUrlHelper(ActionContext context)
            {
                return new MockUrlHelper();
            }
        }

        private class MockUrlHelper : IUrlHelper
        {
            public string Action(UrlActionContext context)
            {
                throw new NotImplementedException();
            }

            public string RouteUrl(UrlRouteContext context)
            {
                return "/mock/url";
            }

            public string Link(string routeName, object values)
            {
                throw new NotImplementedException();
            }

            public bool IsLocalUrl(string url)
            {
                throw new NotImplementedException();
            }

            public string Content(string contentPath)
            {
                throw new NotImplementedException();
            }

            public string Page(string pageName, string pageHandler, object values, string protocol, string host, string fragment)
            {
                throw new NotImplementedException();
            }
        }
    }
}
