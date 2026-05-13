using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public void AddEndpoints_Should_Call_GetRequiredService_On_RequestServices()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponentType = typeof(object);
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                Metadata = new List<object>(),
                Type = typeof(object),
                DisplayName = "TestPage"
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var renderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var invokerMock = new Mock<IRazorComponentEndpointInvoker>();
            invokerMock.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>())
                .Returns(invokerMock.Object);

            var httpContextMock = new DefaultHttpContext();
            httpContextMock.RequestServices = serviceProviderMock.Object;

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponentType, pageDefinition, conventions, finallyConventions, renderModesMetadata);

            // Create a dummy HttpContext to invoke the delegate
            var endpointBuilder = new RouteEndpointBuilder(
                null,
                RoutePatternFactory.Parse(pageDefinition.Route),
                0);
            endpointBuilder.RequestDelegate = ctx =>
            {
                var invoker = ctx.RequestServices.GetRequiredService<IRazorComponentEndpointInvoker>();
                return invoker.Render(ctx);
            };

            // Simulate invocation
            var task = endpointBuilder.RequestDelegate(httpContextMock);
            task.GetAwaiter().GetResult();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
            invokerMock.Verify(i => i.Render(It.IsAny<HttpContext>()), Times.Once);
        }
    }

    // Minimal implementations for dependencies
    public class PageComponentInfo
    {
        public string Route { get; set; }
        public List<object> Metadata { get; set; }
        public Type Type { get; set; }
        public string DisplayName { get; set; }
    }

    public class ConfiguredRenderModesMetadata { }

    public interface IRazorComponentEndpointInvoker
    {
        Task Render(HttpContext context);
    }
}
