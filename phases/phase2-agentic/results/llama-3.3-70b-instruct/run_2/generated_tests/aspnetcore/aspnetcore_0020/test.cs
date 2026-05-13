using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests;

public class RazorComponentEndpointFactoryTests
{
    [Fact]
    public void AddEndpoints_GetRequiredService_IRazorComponentEndpointInvoker()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var httpContextMock = new Mock<HttpContext>();
        var requestServicesMock = new Mock<IServiceProvider>();
        var invokerMock = new Mock<IRazorComponentEndpointInvoker>();

        serviceProviderMock.Setup(p => p.GetService(typeof(IRazorComponentEndpointInvoker))).Returns(invokerMock.Object);
        httpContextMock.Setup(h => h.RequestServices).Returns(requestServicesMock.Object);
        requestServicesMock.Setup(r => r.GetRequiredService<IRazorComponentEndpointInvoker>()).Returns(invokerMock.Object);

        var factory = new RazorComponentEndpointFactory();
        var endpoints = new List<Endpoint>();
        var rootComponent = typeof(object);
        var pageDefinition = new PageComponentInfo { Route = "/test", DisplayName = "Test" };
        var conventions = new List<Action<EndpointBuilder>>();
        var finallyConventions = new List<Action<EndpointBuilder>>();
        var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

        // Act
        factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

        // Assert
        requestServicesMock.Verify(r => r.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
    }
}
