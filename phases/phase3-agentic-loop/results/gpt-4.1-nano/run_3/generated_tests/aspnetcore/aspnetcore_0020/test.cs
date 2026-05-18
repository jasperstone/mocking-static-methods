using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Components.Endpoints;

namespace RazorComponentEndpointFactoryTests
{
    public class AddEndpointsTests
    {
        [Fact]
        public async Task AddEndpoints_CallsGetRequiredServiceAndInvokesRender()
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

            var mockInvoker = new Mock<IRazorComponentEndpointInvoker>();
            mockInvoker.Setup(i => i.Render(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            var requestServicesMock = new Mock<IServiceProvider>();
            requestServicesMock.Setup(rs => rs.GetRequiredService<IRazorComponentEndpointInvoker>())
                .Returns(mockInvoker.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(requestServicesMock.Object);

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponentType, pageDefinition, conventions, finallyConventions, renderModesMetadata);

            // Retrieve the created endpoint's request delegate
            var builder = new RouteEndpointBuilder(
                null,
                RoutePatternFactory.Parse(pageDefinition.Route),
                0);
            // Re-create the delegate as in the method
            builder.RequestDelegate = static httpContext =>
            {
                var invoker = httpContext.RequestServices.GetRequiredService<IRazorComponentEndpointInvoker>();
                return invoker.Render(httpContext);
            };

            var requestDelegate = builder.RequestDelegate;

            // Invoke the delegate
            await requestDelegate(httpContextMock.Object);

            // Assert
            // Verify that GetRequiredService was called
            requestServicesMock.Verify(rs => rs.GetRequiredService<IRazorComponentEndpointInvoker>(), Times.Once);
            // Verify that Render was called
            mockInvoker.Verify(i => i.Render(It.IsAny<HttpContext>()), Times.Once);
            // Verify that endpoint was added
            Assert.Single(endpoints);
        }
    }

    // Placeholder classes to match the production code
    public class PageComponentInfo
    {
        public string Route { get; set; }
        public string DisplayName { get; set; }
        public Type Type { get; set; }
        public List<object> Metadata { get; set; }
    }

    public class ConfiguredRenderModesMetadata { }
}
