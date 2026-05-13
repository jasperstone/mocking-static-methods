using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        private class DummyTransformer : DynamicRouteValueTransformer
        {
            public object? State { get; set; }
            public override Task<RouteValueDictionary?> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return Task.FromResult<RouteValueDictionary?>(new RouteValueDictionary { { "key", "value" } });
            }
            public override Task<IReadOnlyList<Endpoint>> FilterAsync(HttpContext httpContext, RouteValueDictionary values, IReadOnlyList<Endpoint> endpoints)
            {
                return Task.FromResult<IReadOnlyList<Endpoint>>(endpoints);
            }
        }

        [Fact]
        public async Task ApplyAsync_CallsGetRequiredServiceAndTransformAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var transformer = new DummyTransformer();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(typeof(DummyTransformer), provider => transformer);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var endpointMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(DummyTransformer), null);
            var endpoint = new Endpoint(
                requestDelegate: null,
                metadata: new[] { endpointMetadata },
                displayName: "TestEndpoint");

            var candidates = new CandidateSet(1);
            candidates.SetValidity(0, true);
            candidates[0] = new CandidateEndpoint(endpoint, new RouteValueDictionary());

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            Assert.NotNull(candidates[0].Values);
            Assert.Contains("key", candidates[0].Values.Keys);
            Assert.Equal("value", candidates[0].Values["key"]);
        }
    }
}
