using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task GetRequiredService_ThrowsInvalidOperationException_WhenTransformerHasState()
        {
            // Arrange
            var services = new ServiceCollection();
            var badTransformer = new BadStateTransformer();
            services.AddSingleton<DynamicRouteValueTransformer>(badTransformer);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var endpoint = CreateTransformerEndpoint(typeof(BadStateTransformer));

            var candidates = new CandidateSet(1);
            candidates.SetValidCandidate(0, endpoint, new RouteValueDictionary());

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => policy.ApplyAsync(httpContext, candidates));

            Assert.Contains("StateShouldBeNullForRouteValueTransformers", exception.Message);
        }

        [Fact]
        public async Task GetRequiredService_Succeeds_WhenTransformerHasNoState()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<GoodTransformer>();
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var endpoint = CreateTransformerEndpoint(typeof(GoodTransformer));

            var candidates = new CandidateSet(1);
            candidates.SetValidCandidate(0, endpoint, new RouteValueDictionary());

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert - no exception thrown, GetRequiredService succeeded
        }

        private static Endpoint CreateTransformerEndpoint(Type transformerType)
        {
            var metadata = new DynamicControllerRouteValueTransformerMetadata(transformerType, "test-state");
            var endpoint = new Endpoint(
                delegate { throw new NotImplementedException(); },
                new EndpointMetadataCollection(new object[] { metadata }),
                "test");

            return endpoint;
        }

        private class BadStateTransformer : DynamicRouteValueTransformer
        {
            public BadStateTransformer()
            {
                State = new object(); // Has state already
            }

            public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
            }
        }

        private class GoodTransformer : DynamicRouteValueTransformer
        {
            public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
            }
        }
    }
}
