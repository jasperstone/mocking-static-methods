using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Components.Endpoints;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace RazorComponentEndpointFactoryTests
{
    public class AddEndpointsTests
    {
        [Fact]
        public async Task AddEndpoints_Should_Call_GetRequiredService_And_Render()
        {
            // Arrange
            var endpoints = new List<Endpoint>();
            var rootComponentType = typeof(object);
            var pageDefinition = new PageComponentInfo
            {
                Route = "/test",
                DisplayName = "TestPage",
                Type = typeof(object),
                Metadata = new List<object>()
            };
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var renderModesMetadata = new ConfiguredRenderModesMetadata();

            var serviceCollection = new ServiceCollection();
            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockInvoker.Setup(i => i.Render(It.IsAny<HttpContext>())).ReturnsAsync("rendered");
            serviceCollection.AddTransient(_ => mockInvoker.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponentType, pageDefinition, conventions, finallyConventions, renderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0];
            var requestDelegate = endpoint.RequestDelegate;
            var result = await requestDelegate.Invoke(context);
            Assert.Equal("rendered", result);
        }
    }

    // Minimal implementations for used types
    public class PageComponentInfo
    {
        public string Route { get; set; }
        public string DisplayName { get; set; }
        public Type Type { get; set; }
        public List<object> Metadata { get; set; }
    }

    public class ConfiguredRenderModesMetadata { }

    public interface IRazorComponentEndpointInvoker
    {
        Task<string> Render(HttpContext context);
    }
}
