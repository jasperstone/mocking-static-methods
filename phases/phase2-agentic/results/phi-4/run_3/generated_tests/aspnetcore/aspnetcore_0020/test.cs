using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
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
        public void AddEndpoints_CallsGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object); // Dummy type for testing
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                Metadata = new List<object>(),
                Type = typeof(object), // Dummy type for testing
                DisplayName = "TestPage"
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>())
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
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
            mockInvoker.Verify(invoker => invoker.Render(It.IsAny<HttpContext>()), Times.Once);
        }
    }
}
