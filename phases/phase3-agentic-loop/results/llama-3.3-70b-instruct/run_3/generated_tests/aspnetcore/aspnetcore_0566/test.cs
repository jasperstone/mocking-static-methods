using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
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
            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory, MockUrlHelperFactory>();
            var serviceProvider = services.BuildServiceProvider();

            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
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
            public string Action(UrlActionContext context) => string.Empty;

            public string RouteUrl(UrlRouteContext context) => "https://example.com";

            public string Link(string routeName, object values) => string.Empty;

            public bool IsLocalUrl(string url) => true;

            public string Content(string contentPath) => string.Empty;
        }
    }
}
