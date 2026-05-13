using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public void AddEndpoints_ShouldAddEndpointWithCorrectMetadata()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                DisplayName = "Test Page",
                Metadata = new List<object> { new TestMetadata() },
                Type = typeof(object)
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0] as RouteEndpoint;
            Assert.NotNull(endpoint);
            Assert.Equal("/test", endpoint.RoutePattern.RawText);
            Assert.Equal("Test Page", endpoint.DisplayName);
            Assert.Equal(0, endpoint.Order);
            Assert.Contains(endpoint.Metadata, m => m is RequireAntiforgeryTokenAttribute);
            Assert.Contains(endpoint.Metadata, m => m is SuppressLinkGenerationMetadata);
            Assert.Contains(endpoint.Metadata, m => m is ComponentTypeMetadata);
            Assert.Contains(endpoint.Metadata, m => m is RootComponentMetadata);
            Assert.Contains(endpoint.Metadata, m => m is ConfiguredRenderModesMetadata);
            Assert.Contains(endpoint.Metadata, m => m is TestMetadata);
        }

        [Fact]
        public void AddEndpoints_ShouldCallGetRequiredServiceOnRequestServices()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                DisplayName = "Test Page",
                Metadata = new List<object>(),
                Type = typeof(object)
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var factory = new RazorComponentEndpointFactory();

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>()).Returns(mockInvoker.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);
            var endpoint = endpoints[0] as RouteEndpoint;
            endpoint.RequestDelegate(httpContext);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
            mockInvoker.Verify(invoker => invoker.Render(httpContext), Times.Once);
        }

        private class TestMetadata { }
    }
}
