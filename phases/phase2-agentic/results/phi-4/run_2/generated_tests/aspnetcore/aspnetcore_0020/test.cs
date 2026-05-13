using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public void AddEndpoints_ShouldInvokeRenderOnRazorComponentEndpointInvoker()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                Metadata = new List<object>(),
                Type = typeof(object),
                DisplayName = "TestPage"
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockInvoker
                .Setup(invoker => invoker.Render(It.IsAny<HttpContext>()))
                .Returns((HttpContext context) => Task.FromResult(context.Response.WriteAsync("Rendered")));

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(serviceProvider => serviceProvider.GetRequiredService<IRazorComponentEndpointInvoker>())
                .Returns(mockInvoker.Object);

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(
                endpoints,
                rootComponent,
                pageDefinition,
                conventions,
                finallyConventions,
                configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints.First();
            var context = new DefaultHttpContext();
            context.RequestServices = mockServiceProvider.Object;

            var task = endpoint.Handler(context);
            task.Wait();

            Assert.Equal("Rendered", context.Response.Body.AsString());
            mockInvoker.Verify(invoker => invoker.Render(context), Times.Once);
        }
    }

    // Helper extension method to read response body as string
    public static class HttpResponseExtensions
    {
        public static string AsString(this HttpResponse response)
        {
            response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            return new System.IO.StreamReader(response.Body).ReadToEnd();
        }
    }
}
