using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests;

public class RazorComponentEndpointFactoryTests
{
    [Fact]
    public void AddEndpoints_GetRequiredService_CallsRender()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        var pageDefinition = new PageComponentInfo("route", "displayname", typeof(object));
        var conventions = new List<Action<EndpointBuilder>>();
        var finallyConventions = new List<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
        serviceProvider.GetService<IRazorComponentEndpointInvoker>(invokerMock.Object);

        // Act
        var factory = new RazorComponentEndpointFactory();
        factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

        // Assert
        invokerMock.Verify(i => i.Render(httpContext), Times.Once);
    }

    [Fact]
    public void AddEndpoints_GetRequiredService_ThrowsIfServiceNotRegistered()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        var pageDefinition = new PageComponentInfo("route", "displayname", typeof(object));
        var conventions = new List<Action<EndpointBuilder>>();
        var finallyConventions = new List<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;

        // Act and Assert
        var factory = new RazorComponentEndpointFactory();
        Assert.Throws<InvalidOperationException>(() => factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata));
    }
}

public class PageComponentInfo
{
    public string Route { get; set; }
    public string DisplayName { get; set; }
    public Type Type { get; set; }
    public List<object> Metadata { get; set; }

    public PageComponentInfo(string route, string displayName, Type type)
    {
        Route = route;
        DisplayName = displayName;
        Type = type;
        Metadata = new List<object>();
    }
}

public class ConfiguredRenderModesMetadata
{
}
