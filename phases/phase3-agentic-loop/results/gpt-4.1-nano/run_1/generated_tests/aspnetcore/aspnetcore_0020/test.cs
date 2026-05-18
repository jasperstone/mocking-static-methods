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
        public void AddEndpoints_Should_Call_GetRequiredService_And_Invoke_Render()
        {
            // Arrange
            var factory = new RazorComponentEndpointFactory();

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

            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockInvoker.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(_ => mockInvoker.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;

            // Act
            factory.AddEndpoints(endpoints, rootComponentType, pageDefinition, conventions, finallyConventions, renderModesMetadata);

            // Assert
            Assert.Single(endpoints);
            var endpoint = endpoints[0];
            var requestDelegate = endpoint.RequestDelegate;
            Assert.NotNull(requestDelegate);

            // Create a mock HttpContext to test the delegate
            var testContext = new DefaultHttpContext();
            testContext.RequestServices = serviceProvider;

            // Invoke the delegate
            var task = requestDelegate(testContext);
            task.GetAwaiter().GetResult();

            // Verify that Render was called
            mockInvoker.Verify(i => i.Render(It.IsAny<HttpContext>()), Times.Once);
        }
    }

    // Minimal implementations for dependencies
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
        Task Render(HttpContext context);
    }
}
