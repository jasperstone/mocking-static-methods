using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private class TestRazorComponentEndpointInvoker : IRazorComponentEndpointInvoker
        {
            public bool RenderCalled { get; private set; }
            public HttpContext? RenderHttpContext { get; private set; }

            public Task Render(HttpContext httpContext)
            {
                RenderCalled = true;
                RenderHttpContext = httpContext;
                return Task.CompletedTask;
            }
        }

        private class DummyPageComponentInfo : PageComponentInfo
        {
            public DummyPageComponentInfo(string route, Type type, string displayName)
                : base(route, type, displayName, Array.Empty<object>())
            {
            }
        }

        [Fact]
        public async Task AddEndpoints_SetsRequestDelegate_And_InvokesRender()
        {
            // Arrange
            var factory = new RazorComponentEndpointFactory();
            var endpoints = new List<Endpoint>();
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata(Array.Empty<object>());

            var pageDefinition = new DummyPageComponentInfo("/test-route", typeof(object), "TestDisplayName");

            var invoker = new TestRazorComponentEndpointInvoker();

            var services = new ServiceCollection();
            services.AddSingleton<IRazorComponentEndpointInvoker>(invoker);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            factory.AddEndpoints(
                endpoints,
                typeof(object),
                pageDefinition,
                conventions,
                finallyConventions,
                configuredRenderModesMetadata);

            Assert.Single(endpoints);
            var endpoint = endpoints[0];

            // The RequestDelegate should be set and invoke the invoker.Render
            var requestDelegate = endpoint.RequestDelegate;
            Assert.NotNull(requestDelegate);

            // Invoke the RequestDelegate and verify Render is called
            await requestDelegate!(httpContext);

            Assert.True(invoker.RenderCalled);
            Assert.Same(httpContext, invoker.RenderHttpContext);

            // Verify metadata contains expected types
            Assert.Contains(endpoint.Metadata, m => m is RequireAntiforgeryTokenAttribute);
            Assert.Contains(endpoint.Metadata, m => m is SuppressLinkGenerationMetadata);
            Assert.Contains(endpoint.Metadata, m => m is ComponentTypeMetadata);
            Assert.Contains(endpoint.Metadata, m => m is RootComponentMetadata);
            Assert.Contains(endpoint.Metadata, m => m == configuredRenderModesMetadata);

            // Verify endpoint order and display name
            Assert.Equal(0, endpoint.Order);
            Assert.Equal("/test-route (TestDisplayName)", endpoint.DisplayName);
        }
    }
}
