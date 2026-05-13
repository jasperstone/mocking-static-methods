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
                return Task.FromResult<RouteValueDictionary?>(new RouteValueDictionary { { "transformed", "value" } });
            }
            public override Task<IReadOnlyList<Endpoint>> FilterAsync(HttpContext httpContext, RouteValueDictionary values, IReadOnlyList<Endpoint> endpoints)
            {
                return Task.FromResult<IReadOnlyList<Endpoint>>(endpoints);
            }
        }

        [Fact]
        public async Task ApplyAsync_CallsGetRequiredServiceAndTransforms()
        {
            // Arrange
            var services = new ServiceCollection();
            var transformer = new DummyTransformer();
            services.AddTransient(typeof(DummyTransformer), provider => transformer);
            var serviceProvider = services.BuildServiceProvider();

            var endpointMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(DummyTransformer), null);
            var endpoint = new Endpoint(
                requestDelegate: null,
                metadata: new[] { endpointMetadata },
                displayName: "TestEndpoint");

            var candidate = new CandidateEndpoint
            {
                Endpoint = endpoint,
                Values = new RouteValueDictionary { { "key", "value" } }
            };

            var candidates = new CandidateSet(new[] { candidate });
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            Assert.NotNull(transformer.State);
            Assert.Contains("transformed", candidates[0].Values.Keys);
            Assert.Equal("value", candidates[0].Values["transformed"]);
        }
    }

    // Helper classes for test
    internal class CandidateEndpoint
    {
        public Endpoint Endpoint { get; set; }
        public RouteValueDictionary? Values { get; set; }
    }

    internal class CandidateSet
    {
        private readonly List<CandidateEndpoint> _candidates;
        public CandidateSet(IEnumerable<CandidateEndpoint> candidates)
        {
            _candidates = new List<CandidateEndpoint>(candidates);
        }
        public int Count => _candidates.Count;
        public CandidateEndpoint this[int index] => _candidates[index];
        public bool IsValidCandidate(int index) => true;
        public void ReplaceEndpoint(int index, Endpoint endpoint, RouteValueDictionary? values)
        {
            _candidates[index] = new CandidateEndpoint { Endpoint = endpoint, Values = values };
        }
        public RouteValueDictionary? this[int index] => _candidates[index].Values;
        public void ExpandEndpoint(int index, IReadOnlyList<Endpoint> endpoints, IEndpointComparer comparer)
        {
            // No-op for test
        }
    }
}
