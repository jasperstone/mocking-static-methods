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

            // Assert
            // Since the method is static, we need to simulate the request delegate invocation
            var endpoint = endpoints[0];
            var requestDelegate = endpoint.RequestDelegate;
            var context = new DefaultHttpContext();
            context.RequestServices = serviceProviderMock.Object;

            // Invoke the request delegate to ensure GetRequiredService is called
            var task = requestDelegate.Invoke(context);
            task.GetAwaiter().GetResult();

            // Verify that GetRequiredService was called
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
        }
    }

    // Minimal placeholder classes to support the test
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
