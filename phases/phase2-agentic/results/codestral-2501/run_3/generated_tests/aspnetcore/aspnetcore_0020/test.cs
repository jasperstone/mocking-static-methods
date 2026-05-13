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
                Type = typeof(object),
                Metadata = new List<object>()
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(h => h.RequestServices).Returns(mockServiceProvider.Object);

            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>()).Returns(mockInvoker.Object);

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
        }
    }
}
