using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_WhenDynamicControllerRouteValueTransformerMetadataPresent_UsesGetRequiredService()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new Mock<IServiceProvider>();
            var transformer = new Mock<DynamicRouteValueTransformer>();
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata
            {
                SelectorType = typeof(DynamicRouteValueTransformer)
            };

            serviceProvider
                .Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)))
                .Returns(transformer.Object);

            httpContext.RequestServices = serviceProvider.Object;

            var endpoint = new Endpoint(
                metadata: new List<object> { transformerMetadata },
                routePattern: null,
                httpMethod: null,
                displayName: null,
                order: 0,
                dataTokens: null,
                target: null);

            var candidates = new CandidateSet(
                new[]
                {
                    new Candidate(endpoint, new RouteValueDictionary { { "key", "value" } })
                });

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            transformer.Verify(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()), Times.Once);
        }
    }

    // Mock classes for testing
    public class DynamicControllerEndpointSelectorCache
    {
        public DynamicControllerEndpointSelector GetEndpointSelector(Endpoint endpoint) => null;
    }

    public class EndpointMetadataComparer
    {
    }

    public class DynamicControllerRouteValueTransformerMetadata
    {
        public Type SelectorType { get; set; }
        public object State { get; set; }
    }

    public class DynamicRouteValueTransformer
    {
        public object? State { get; set; }

        public virtual ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
        }

        public virtual ValueTask<IReadOnlyList<Endpoint>> FilterAsync(HttpContext httpContext, RouteValueDictionary values, IReadOnlyList<Endpoint> endpoints)
        {
            return new ValueTask<IReadOnlyList<Endpoint>>(endpoints);
        }
    }

    // Additional required classes
    public class DynamicControllerEndpointSelector
    {
        public IReadOnlyList<Endpoint> SelectEndpoints(RouteValueDictionary values) => Array.Empty<Endpoint>();
    }

    public class Candidate
    {
        public Endpoint Endpoint { get; }
        public RouteValueDictionary Values { get; }

        public Candidate(Endpoint endpoint, RouteValueDictionary values)
        {
            Endpoint = endpoint;
            Values = values;
        }
    }

    public class CandidateSet
    {
        private readonly Candidate[] _candidates;

        public CandidateSet(Candidate[] candidates)
        {
            _candidates = candidates;
        }

        public int Count => _candidates.Length;

        public bool IsValidCandidate(int index) => index >= 0 && index < _candidates.Length;

        public void ReplaceEndpoint(int index, Endpoint endpoint, RouteValueDictionary values)
        {
            // Implementation not needed for this test
        }

        public void ExpandEndpoint(int index, IReadOnlyList<Endpoint> endpoints, EndpointMetadataComparer comparer)
        {
            // Implementation not needed for this test
        }
    }
}
