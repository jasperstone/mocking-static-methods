using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public async Task AddEndpoints_GetRequiredService_CallsRender()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IRazorComponentEndpointInvoker>(Mock.Of<IRazorComponentEndpointInvoker>())
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };

            var pageDefinition = new PageComponentInfo
            {
                Route = "/",
                DisplayName = "Test Page",
                Type = typeof(TestPage),
                Metadata = new[]
                {
                    new RequireAntiforgeryTokenAttribute(),
                },
            };

            var rootComponent = typeof(TestRootComponent);

            var conventions = new Action<EndpointBuilder>[0];
            var finallyConventions = new Action<EndpointBuilder>[0];
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var endpoints = new List<Endpoint>();

            var factory = new RazorComponentEndpointFactory();

            // Act
            factory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            var endpoint = endpoints[0];
            await endpoint.RequestDelegate(httpContext);

            var invoker = (Mock<IRazorComponentEndpointInvoker>)serviceProvider.GetService<IRazorComponentEndpointInvoker>();
            invoker.Verify(i => i.Render(httpContext), Times.Once);
        }
    }

    public class TestPage { }

    public class TestRootComponent { }
}
