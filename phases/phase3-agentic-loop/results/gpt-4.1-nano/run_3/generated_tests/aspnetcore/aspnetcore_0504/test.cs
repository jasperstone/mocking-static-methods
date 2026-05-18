using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    // Minimal implementation for the test
    public class DynamicControllerRouteValueTransformerMetadata
    {
        public Type SelectorType { get; set; }
        public object? State { get; set; }
    }

    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_CallsGetRequiredService_WhenTransformMetadataExists()
        {
            // Arrange
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            mockTransformer.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary());

            var services = new ServiceCollection()
                .AddTransient(_ => mockTransformer.Object.GetType(), _ => mockTransformer.Object)
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();

            // Setup RequestServices to return the mock transformer when requested
            var mockRequestServices = new Mock<IServiceProvider>();
            mockRequestServices.Setup(s => s.GetRequiredService(It.Is<Type>(t => t == mockTransformer.Object.GetType())))
                .Returns(mockTransformer.Object);
            httpContext.RequestServices = mockRequestServices.Object;

            var endpointMetadata = new List<object>
            {
                new DynamicControllerRouteValueTransformerMetadata
                {
                    SelectorType = mockTransformer.Object.GetType(),
                    State = null
                }
            };

            var endpoint = new Endpoint(
                c => Task.CompletedTask,
                new EndpointMetadataCollection(endpointMetadata),
                "TestEndpoint");

            var candidate = new CandidateEndpoint(endpoint);
            var candidates = new CandidateSet(new[] { candidate });
            var matcherPolicy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await matcherPolicy.ApplyAsync(httpContext, candidates);

            // Assert
            mockTransformer.Verify(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()), Times.Once);
        }

        // Helper classes to mock CandidateSet and CandidateEndpoint
        private class CandidateEndpoint
        {
            public Endpoint Endpoint { get; }
            public RouteValueDictionary? Values { get; }

            public CandidateEndpoint(Endpoint endpoint)
            {
                Endpoint = endpoint;
                Values = new RouteValueDictionary();
            }
        }

        private class CandidateSet
        {
            private readonly List<CandidateEndpoint> _candidates;

            public CandidateSet(IEnumerable<CandidateEndpoint> candidates)
            {
                _candidates = new List<CandidateEndpoint>(candidates);
            }

            public int Count => _candidates.Count;

            public bool IsValidCandidate(int index) => true;

            public CandidateEndpoint this[int index] => _candidates[index];

            public void ReplaceEndpoint(int index, Endpoint? endpoint, RouteValueDictionary? values)
            {
                _candidates[index] = new CandidateEndpoint(endpoint ?? _candidates[index].Endpoint)
                {
                    Values = values
                };
            }

            public void ExpandEndpoint(int index, IEnumerable<Endpoint> endpoints, EndpointMetadataComparer comparer)
            {
                // No-op for test
            }
        }
    }
}
