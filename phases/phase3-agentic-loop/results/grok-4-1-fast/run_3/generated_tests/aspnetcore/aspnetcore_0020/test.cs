using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests;

public class RazorComponentEndpointFactoryTests
{
    [Fact]
    public async Task AddEndpoints_CreatesRequestDelegate_ThatCallsGetRequiredService()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        
        var pageDefinition = new Mock<PageComponentInfo>();
        pageDefinition.Setup(p => p.Route).Returns("/test");
        pageDefinition.Setup(p => p.Type).Returns(typeof(object));
        pageDefinition.Setup(p => p.Metadata).Returns(Array.Empty<object>());
        pageDefinition.Setup(p => p.DisplayName).Returns("TestPage");
        
        var conventions = Array.Empty<Action<EndpointBuilder>>();
        var finallyConventions = Array.Empty<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<ConfiguredRenderMode>());

        var getRequiredServiceCalled = false;
        var mockRequestServices = new Mock<IServiceProvider>();
        mockRequestServices
            .Setup(s => s.GetRequiredService<IRazorComponentEndpointInvoker>())
            .Callback(() => getRequiredServiceCalled = true)
            .Returns(Mock.Of<IRazorComponentEndpointInvoker>());

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = mockRequestServices.Object;

        var factory = new RazorComponentEndpointFactory();

        // Act
        factory.AddEndpoints(
            endpoints,
            rootComponent,
            pageDefinition.Object,
            conventions,
            finallyConventions,
            configuredRenderModesMetadata);

        var endpoint = Assert.Single(endpoints);
        var requestDelegate = endpoint.RequestDelegate;

        // Assert
        Assert.NotNull(requestDelegate);

        // Act - invoke to exercise the GetRequiredService call (line 56)
        await requestDelegate(httpContext);

        // Assert - verifies coverage of GetRequiredService call
        Assert.True(getRequiredServiceCalled);
        mockRequestServices.Verify(s => s.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once());
    }

    [Fact]
    public async Task AddEndpoints_RequestDelegate_ThrowsInvalidOperationException_WhenInvokerMissing()
    {
        // Arrange
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        
        var pageDefinition = new Mock<PageComponentInfo>();
        pageDefinition.Setup(p => p.Route).Returns("/test");
        pageDefinition.Setup(p => p.Type).Returns(typeof(object));
        pageDefinition.Setup(p => p.Metadata).Returns(Array.Empty<object>());
        pageDefinition.Setup(p => p.DisplayName).Returns("TestPage");
        
        var conventions = Array.Empty<Action<EndpointBuilder>>();
        var finallyConventions = Array.Empty<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<ConfiguredRenderMode>());

        var mockRequestServices = new Mock<IServiceProvider>();
        mockRequestServices
            .Setup(s => s.GetRequiredService<IRazorComponentEndpointInvoker>())
            .Throws(new InvalidOperationException("Unable to resolve service for type 'IRazorComponentEndpointInvoker' while attempting to activate the service."));

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = mockRequestServices.Object;

        var factory = new RazorComponentEndpointFactory();

        // Act
        factory.AddEndpoints(
            endpoints,
            rootComponent,
            pageDefinition.Object,
            conventions,
            finallyConventions,
            configuredRenderModesMetadata);

        var endpoint = Assert.Single(endpoints);
        var requestDelegate = endpoint.RequestDelegate;

        // Act & Assert - verifies GetRequiredService throws as expected
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => requestDelegate(httpContext));
        Assert.Equal("Unable to resolve service for type 'IRazorComponentEndpointInvoker' while attempting to activate the service.", exception.Message);
        mockRequestServices.Verify(s => s.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once());
    }
}
