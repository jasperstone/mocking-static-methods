using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_CallsGetRequiredService_ForTransformer()
        {
            // Arrange
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            mockTransformer.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary { { "foo", "bar" } });
            mockTransformer.Setup(t => t.State).Returns(null);
            mockTransformer.SetupSet(t => t.State = It.IsAny<object>());

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(typeof(DynamicRouteValueTransformer), provider => mockTransformer.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var endpointMetadata = new List<object>
            {
                new DynamicControllerRouteValueTransformerMetadata
                {
                    SelectorType = typeof(DynamicRouteValueTransformer),
                    State = null
                }
            };

            var endpoint = new Endpoint(
                requestDelegate: null,
                metadata: endpointMetadata,
                displayName: "TestEndpoint");

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var candidate = new CandidateEndpoint(endpoint, new RouteValueDictionary(), true);
            var candidates = new CandidateSet(new[] { candidate });

            var selectorCacheMock = new Mock<DynamicControllerEndpointSelectorCache>();
            var comparer = new EndpointMetadataComparer();

            var policy = new DynamicControllerEndpointMatcherPolicy(selectorCacheMock.Object, comparer);

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            mockTransformer.Verify(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()), Times.Once);
        }
    }

    // Helper classes for testing
    internal class CandidateEndpoint
    {
        public Endpoint Endpoint { get; }
        public RouteValueDictionary Values { get; }
        public bool IsValid { get; }

        public CandidateEndpoint(Endpoint endpoint, RouteValueDictionary values, bool isValid)
        {
            Endpoint = endpoint;
            Values = values;
            IsValid = isValid;
        }
    }

    internal class CandidateSet
    {
        private readonly List<CandidateEndpoint> _candidates;

        public int Count => _candidates.Count;

        public CandidateSet(IEnumerable<CandidateEndpoint> candidates)
        {
            _candidates = new List<CandidateEndpoint>(candidates);
        }

        public bool IsValidCandidate(int index) => _candidates[index].IsValid;

        public Endpoint Endpoint(int index) => _candidates[index].Endpoint;

        public RouteValueDictionary Values(int index) => _candidates[index].Values;

        public void ReplaceEndpoint(int index, Endpoint endpoint, RouteValueDictionary values)
        {
            _candidates[index] = new CandidateEndpoint(endpoint ?? _candidates[index].Endpoint, values ?? _candidates[index].Values, _candidates[index].IsValid);
        }

        public void ExpandEndpoint(int index, IEnumerable<Endpoint> endpoints, EndpointMetadataComparer comparer)
        {
            // For simplicity, do nothing
        }
    }
}
