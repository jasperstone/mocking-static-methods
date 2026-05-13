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
        public void GetUrl_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => attribute.GetUrl(null));
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNotFound()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                },
            };
            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(h => h.RouteUrl(It.IsAny<UrlRouteContext>()) == null)));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                attribute.GetUrl(context);
            });
        }

        [Fact]
        public void GetUrl_ReturnsUrl_WhenUrlIsFound()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                },
            };
            var url = "https://example.com/action";
            var services = new ServiceCollection();
            services.AddSingleton<IUrlHelperFactory>(Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(h => h.RouteUrl(It.IsAny<UrlRouteContext>()) == url)));
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var result = attribute.GetUrl(context);

            // Assert
            Assert.Equal(url, result);
        }

        [Fact]
        public void GetUrl_CallsGetRequiredService()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");
            var context = new ClientModelValidationContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                },
            };
            var services = new ServiceCollection();
            var urlHelperFactory = Mock.Of<IUrlHelperFactory>(f => f.GetUrlHelper(It.IsAny<ActionContext>()) == Mock.Of<IUrlHelper>(h => h.RouteUrl(It.IsAny<UrlRouteContext>()) == "https://example.com/action"));
            services.AddSingleton<IUrlHelperFactory>(urlHelperFactory);
            context.ActionContext.HttpContext.RequestServices = services.BuildServiceProvider();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactory);

            // Act
            attribute.GetUrl(context);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IUrlHelperFactory>(), Times.Once);
        }
    }
}
