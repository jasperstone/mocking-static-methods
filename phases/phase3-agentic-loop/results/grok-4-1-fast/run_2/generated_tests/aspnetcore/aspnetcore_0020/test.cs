using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Discovery;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Endpoints;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests;

public class RazorComponentEndpointFactoryTests
{
    [Fact]
    public async Task AddEndpoints_CreatesEndpointWithGetRequiredServiceCall()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        var pageDefinition = new Mock<PageComponentInfo>();
        pageDefinition.Setup(p => p.Route).Returns("/test");
        pageDefinition.Setup(p => p.Type).Returns(typeof(object));
        pageDefinition.Setup(p => p.DisplayName).Returns("TestPage");
        pageDefinition.Setup(p => p.Metadata).Returns(Array.Empty<object>());
        var conventions = Array.Empty<Action<EndpointBuilder>>();
        var finallyConventions = Array.Empty<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<ConfiguredRenderMode>());

        var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
        mockInvoker.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton(mockInvoker.Object);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var factory = new RazorComponentEndpointFactory();

        // Act
        factory.AddEndpoints(endpoints, rootComponent, pageDefinition.Object, conventions, finallyConventions, configuredRenderModesMetadata);

        var endpoint = Assert.Single(endpoints);
        var routeEndpoint = Assert.IsType<RouteEndpoint>(endpoint);
        Assert.NotNull(routeEndpoint.RequestDelegate);

        // Act - execute the delegate to trigger GetRequiredService
        await routeEndpoint.RequestDelegate(httpContext);

        // Assert - verifies GetRequiredService was called indirectly by successful invoker.Render
        mockInvoker.Verify(i => i.Render(httpContext), Times.Once);
    }

    [Fact]
    public async Task AddEndpoints_ThrowsInvalidOperation_WhenInvokerNotRegistered()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        var pageDefinition = new Mock<PageComponentInfo>();
        pageDefinition.Setup(p => p.Route).Returns("/test");
        pageDefinition.Setup(p => p.Type).Returns(typeof(object));
        pageDefinition.Setup(p => p.DisplayName).Returns("TestPage");
        pageDefinition.Setup(p => p.Metadata).Returns(Array.Empty<object>());
        var conventions = Array.Empty<Action<EndpointBuilder>>();
        var finallyConventions = Array.Empty<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<ConfiguredRenderMode>());

        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var factory = new RazorComponentEndpointFactory();

        // Act
        factory.AddEndpoints(endpoints, rootComponent, pageDefinition.Object, conventions, finallyConventions, configuredRenderModesMetadata);

        var endpoint = Assert.Single(endpoints);
        var routeEndpoint = Assert.IsType<RouteEndpoint>(endpoint);

        // Assert - GetRequiredService should throw InvalidOperationException
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => routeEndpoint.RequestDelegate!(httpContext));
        Assert.Contains("IRazorComponentEndpointInvoker", exception.Message);
    }
}
