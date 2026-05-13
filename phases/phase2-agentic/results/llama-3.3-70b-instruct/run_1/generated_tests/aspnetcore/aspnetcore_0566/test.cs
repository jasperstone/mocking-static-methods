using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(u => u.RouteUrl(It.IsAny<UrlRouteContext>()) == null)));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            var attribute = new RemoteAttribute("action", "controller");

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(context));
        }

        [Fact]
        public void GetUrl_ReturnsUrl_WhenUrlHelperReturnsUrl()
        {
            // Arrange
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            var url = "https://example.com";
            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(u => u.RouteUrl(It.IsAny<UrlRouteContext>()) == url)));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            var attribute = new RemoteAttribute("action", "controller");

            // Act
            var result = attribute.GetUrl(context);

            // Assert
            Assert.Equal(url, result);
        }

        [Fact]
        public void GetUrl_UsesRouteNameAndValuesFromAttribute()
        {
            // Arrange
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            var routeName = "routeName";
            var values = new RouteValueDictionary { { "key", "value" } };
            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(u => u.RouteUrl(It.IsAny<UrlRouteContext>()) == "https://example.com")));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            var attribute = new RemoteAttribute("action", "controller")
            {
                RouteName = routeName,
            };
            attribute.RouteData.Add("key", "value");

            // Act
            attribute.GetUrl(context);

            // Assert
            Mock.Get<IUrlHelperFactory>(services.GetService<IUrlHelperFactory>()).Verify(f => f.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
            Mock.Get<IUrlHelper>(Mock.Get<IUrlHelperFactory>(services.GetService<IUrlHelperFactory>()).GetUrlHelper(It.IsAny<ActionContext>())).Verify(u => u.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName == routeName && c.Values.ContainsKey("key") && c.Values["key"].Equals("value"))), Times.Once);
        }

        [Fact]
        public void GetUrl_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };

            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(u => u.RouteUrl(It.IsAny<UrlRouteContext>()) == "https://example.com")));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            var attribute = new RemoteAttribute("action", "controller");

            // Act
            attribute.GetUrl(context);

            // Assert
            var serviceProvider = context.ActionContext.HttpContext.RequestServices;
            Mock.Get<IServiceProvider>(serviceProvider).Verify(s => s.GetRequiredService<IUrlHelperFactory>(), Times.Once);
        }
    }
}
