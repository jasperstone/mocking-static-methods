using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public async Task AddEndpoints_GetRequiredService_CallsRender()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IRazorComponentEndpointInvoker>(provider => new MockRazorComponentEndpointInvoker())
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var pageDefinition = new PageComponentInfo
            {
                Route = "/",
                DisplayName = "Test Page",
                Type = typeof(object),
                Metadata = new List<object>()
            };

            var rootComponent = typeof(object);
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var factory = new RazorComponentEndpointFactory();
            var endpoints = new List<Endpoint>();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            var endpoint = endpoints.First();
            var requestDelegate = (RequestDelegate)endpoint.RequestDelegate;
            await requestDelegate(httpContext);

            var invoker = (MockRazorComponentEndpointInvoker)serviceProvider.GetRequiredService<IRazorComponentEndpointInvoker>();
            Assert.True(invoker.RenderCalled);
        }

        private class MockRazorComponentEndpointInvoker : IRazorComponentEndpointInvoker
        {
            public bool RenderCalled { get; private set; } = false;

            public Task Render(HttpContext httpContext)
            {
                RenderCalled = true;
                return Task.CompletedTask;
            }
        }
    }
}
