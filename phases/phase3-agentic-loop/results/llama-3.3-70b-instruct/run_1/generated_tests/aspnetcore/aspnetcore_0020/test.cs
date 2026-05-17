using Xunit;
using Moq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Discovery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Endpoints;

namespace Microsoft.AspNetCore.Components.Endpoints.Tests
{
    public class RazorComponentEndpointFactoryTests
    {
        [Fact]
        public async Task AddEndpoints_GetRequiredService_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IRazorComponentEndpointInvoker, RazorComponentEndpointInvoker>()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var pageDefinition = new PageComponentInfo("TestPage", "/test", typeof(TestPage));
            var rootComponent = typeof(TestRootComponent);
            var conventions = new List<Action<EndpointBuilder>>();
            var finallyConventions = new List<Action<EndpointBuilder>>();
            var configuredRenderModesMetadata = new ConfiguredRenderModesMetadata();

            var endpointFactory = new RazorComponentEndpointFactory();
            var endpoints = new List<Endpoint>();

            // Act
            endpointFactory.AddEndpoints(endpoints, rootComponent, pageDefinition, conventions, finallyConventions, configuredRenderModesMetadata);

            // Assert
            var invoker = httpContext.RequestServices.GetRequiredService<IRazorComponentEndpointInvoker>();
            Assert.NotNull(invoker);
        }
    }

    public class TestPage : ComponentBase { }

    public class TestRootComponent : ComponentBase { }
}
