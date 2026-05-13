using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public void AddEndpoints_ShouldInvokeGetRequiredService()
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
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(s => s.GetRequiredService<IRazorComponentEndpointInvoker>())
                .Returns(mockInvoker.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };

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
            mockInvoker.Verify(i => i.Render(It.IsAny<HttpContext>()), Times.Once);
        }
    }

    // Mock classes to make the test compile
    public class PageComponentInfo
    {
        public string Route { get; set; }
        public List<object> Metadata { get; set; }
        public Type Type { get; set; }
        public string DisplayName { get; set; }
    }

    public interface IRazorComponentEndpointInvoker
    {
        Task Render(HttpContext httpContext);
    }

    public class ConfiguredRenderModesMetadata
    {
    }
}
