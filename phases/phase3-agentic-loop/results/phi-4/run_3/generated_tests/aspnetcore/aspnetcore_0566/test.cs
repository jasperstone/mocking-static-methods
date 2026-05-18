using System;
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
        public void GetUrl_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.GetUrl(null));
        }

        [Fact]
        public void GetUrl_RetrievesUrlHelperFactoryFromServiceProvider()
        {
            // Arrange
            var context = new ClientModelValidationContext(
                new ActionContext(
                    new DefaultHttpContext(),
                    new RouteData(),
                    new ActionDescriptor()),
                new ModelMetadataProvider(),
                new ModelBindingMessageProvider());

            var factoryMock = new Mock<IUrlHelperFactory>();
            var services = new ServiceCollection()
                .AddSingleton<IUrlHelperFactory>(factoryMock.Object)
                .BuildServiceProvider();

            var attribute = new RemoteAttribute("action", "controller")
            {
                RouteData = new RouteValueDictionary { { "controller", "TestController" }, { "action", "TestAction" } }
            };

            context.ActionContext.HttpContext.RequestServices = services;

            // Act
            var url = attribute.GetUrl(context);

            // Assert
            factoryMock.Verify(f => f.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
        }
    }
}
