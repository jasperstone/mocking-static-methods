using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.NotNull(transformer);
            Assert.Contains("transformed", candidates[0].Values.Values);
        }

        [Fact]
        public async Task ApplyAsync_ThrowsInvalidOperationException_WhenTransformerStateIsNotNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var transformer = new DummyTransformer { State = "not null" };
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

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => policy.ApplyAsync(httpContext, candidates));
        }

        // Additional helper classes for testing
        private class CandidateEndpoint
        {
            public Endpoint Endpoint { get; set; }
            public RouteValueDictionary? Values { get; set; }
        }

        private class CandidateSet : IReadOnlyList<CandidateEndpoint>
        {
            private readonly List<CandidateEndpoint> _candidates;

            public CandidateSet(IEnumerable<CandidateEndpoint> candidates)
            {
                _candidates = candidates.ToList();
            }

            public CandidateEndpoint this[int index] => _candidates[index];

            public int Count => _candidates.Count;

            public IEnumerator<CandidateEndpoint> GetEnumerator() => _candidates.GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _candidates.GetEnumerator();

            public bool IsValidCandidate(int index) => true;

            public void ReplaceEndpoint(int index, Endpoint? endpoint, RouteValueDictionary? values)
            {
                _candidates[index].Endpoint = endpoint;
                _candidates[index].Values = values;
            }

            public void ExpandEndpoint(int index, IReadOnlyList<Endpoint> endpoints, IEndpointComparer comparer)
            {
                // No-op for testing
            }
        }
    }
}
