using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Endpoints;
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
        public async Task AddEndpoints_SetsRequestDelegate_AndInvokesRender()
        {
            // Arrange
            var factory = new RazorComponentEndpointFactory();
            var endpoints = new List<Endpoint>();

            var rootComponent = typeof(object);
            var pageType = typeof(object);
            var pageDefinition = new PageComponentInfo(
                displayName: "TestPage",
                type: pageType,
                route: "/test",
                metadata: new List<object> { new object() });

            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<string>());

            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            var httpContextMock = new DefaultHttpContext();
            httpContextMock.RequestServices = new ServiceCollection()
                .AddSingleton(invokerMock.Object)
                .BuildServiceProvider();

            var expectedResponse = Task.CompletedTask;
            invokerMock.Setup(i => i.Render(httpContextMock)).Returns(expectedResponse).Verifiable();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0];
            Assert.NotNull(endpoint.RequestDelegate);

            // Invoke the RequestDelegate and verify it calls invoker.Render
            var requestDelegate = endpoint.RequestDelegate;
            var task = requestDelegate(httpContextMock);
            Assert.Same(expectedResponse, task);
            invokerMock.Verify();
        }
    }

    // Minimal stub for ConfiguredRenderModesMetadata to allow compilation
    internal class ConfiguredRenderModesMetadata
    {
        public ConfiguredRenderModesMetadata(string[] modes)
        {
            Modes = modes;
        }

        public string[] Modes { get; }
    }

    // Minimal stub for IRazorComponentEndpointInvoker to allow mocking
    internal interface IRazorComponentEndpointInvoker
    {
        Task Render(HttpContext httpContext);
    }
}
