using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public async Task AddEndpoints_GetRequiredService_IRazorComponentEndpointInvoker_Render()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IRazorComponentEndpointInvoker, MockRazorComponentEndpointInvoker>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var pageDefinition = new PageComponentInfo
            {
                Route = "/",
                Type = typeof(object),
                DisplayName = "Test Page",
                Metadata = Array.Empty<object>()
            };

            var rootComponent = typeof(object);
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(new IComponentRenderMode[] { });

            var endpointFactory = new RazorComponentEndpointFactory();
            var endpoints = new List<Endpoint>();

            // Act
            endpointFactory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            var invoker = httpContext.RequestServices.GetRequiredService<IRazorComponentEndpointInvoker>();
            await invoker.Render(httpContext);
        }
    }

    public class MockRazorComponentEndpointInvoker : IRazorComponentEndpointInvoker
    {
        public Task Render(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }
    }
}
