using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Components.Endpoints;
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
            var pageDefinition = new PageComponentInfo(
                "Test Page",
                typeof(object),
                "/test",
                new List<object> { new RequireAntiforgeryTokenAttribute() }
            );
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            invokerMock.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IRazorComponentEndpointInvoker))).Returns(invokerMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

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
            Assert.Contains(endpoint.Metadata, m => m is HttpMethodMetadata);
            Assert.Contains(endpoint.Metadata, m => m is ComponentTypeMetadata);
            Assert.Contains(endpoint.Metadata, m => m is RootComponentMetadata);
            Assert.Contains(endpoint.Metadata, m => m is ConfiguredRenderModesMetadata);
        }

        [Fact]
        public void AddEndpoints_ShouldInvokeConventions()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo(
                "Test Page",
                typeof(object),
                "/test",
                new List<object> { new RequireAntiforgeryTokenAttribute() }
            );
            var conventions = new List<Action<EndpointBuilder>> { builder => builder.DisplayName = "Convention" };
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            invokerMock.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IRazorComponentEndpointInvoker))).Returns(invokerMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0] as RouteEndpoint;
            Assert.NotNull(endpoint);
            Assert.Equal("Convention", endpoint.DisplayName);
        }

        [Fact]
        public void AddEndpoints_ShouldInvokeFinallyConventions()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo(
                "Test Page",
                typeof(object),
                "/test",
                new List<object> { new RequireAntiforgeryTokenAttribute() }
            );
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>> { builder => builder.DisplayName = "Finally Convention" };
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            invokerMock.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IRazorComponentEndpointInvoker))).Returns(invokerMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0] as RouteEndpoint;
            Assert.NotNull(endpoint);
            Assert.Equal("Finally Convention", endpoint.DisplayName);
        }

        [Fact]
        public void AddEndpoints_ShouldInvokeRequestDelegate()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponent = typeof(object);
            var pageDefinition = new PageComponentInfo(
                "Test Page",
                typeof(object),
                "/test",
                new List<object> { new RequireAntiforgeryTokenAttribute() }
            );
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            invokerMock.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IRazorComponentEndpointInvoker))).Returns(invokerMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0] as RouteEndpoint;
            Assert.NotNull(endpoint);
            var requestDelegate = endpoint.RequestDelegate;
            Assert.NotNull(requestDelegate);
            requestDelegate(httpContextMock.Object).Wait();
            invokerMock.Verify(i => i.Render(httpContextMock.Object), Times.Once);
        }
    }
}
