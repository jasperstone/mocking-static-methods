using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Tests.Routing
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_CallsGetRequiredServiceOnRequestServices_WithTransformerType()
        {
            // Arrange
            var selectorCache = new DynamicControllerEndpointSelectorCacheForTest();
            var comparer = new EndpointMetadataComparer();
            var policy = new DynamicControllerEndpointMatcherPolicy(selectorCache, comparer);

            var httpContext = new DefaultHttpContext();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var transformerMock = new Mock<DynamicRouteValueTransformer>();
            transformerMock.SetupProperty(t => t.State, null);
            transformerMock.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary(new Dictionary<string, object> { { "key", "value" } }));
            transformerMock.Setup(t => t.FilterAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>(), It.IsAny<IReadOnlyList<Endpoint>>()))
                .ReturnsAsync(new List<Endpoint> { new Endpoint((context) => Task.CompletedTask, new EndpointMetadataCollection(), "test") });

            var transformerType = typeof(TestTransformer);
            serviceProviderMock.Setup(sp => sp.GetService(transformerType)).Returns(transformerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(transformerType)).Returns(transformerMock.Object);

            httpContext.RequestServices = serviceProviderMock.Object;

            var endpoint = new Endpoint(
                (context) => Task.CompletedTask,
                new EndpointMetadataCollection(
                    new DynamicControllerRouteValueTransformerMetadata(transformerType, state: null)
                ),
                "test");

            var candidates = new CandidateSet(new[] { endpoint }, new RouteValueDictionary[] { new RouteValueDictionary() });

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(transformerType), Times.Once);
            Assert.NotNull(candidates[0].Values);
            Assert.Equal("value", candidates[0].Values["key"]);
        }

        private class TestTransformer : DynamicRouteValueTransformer
        {
            public override ValueTask<RouteValueDictionary?> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return new ValueTask<RouteValueDictionary?>(new RouteValueDictionary(new Dictionary<string, object> { { "key", "value" } }));
            }
        }

        private class DynamicControllerEndpointSelectorCacheForTest : DynamicControllerEndpointSelectorCache
        {
            public override DynamicControllerEndpointSelector GetEndpointSelector(Endpoint endpoint)
            {
                return new DynamicControllerEndpointSelectorForTest();
            }
        }

        private class DynamicControllerEndpointSelectorForTest : DynamicControllerEndpointSelector
        {
            public DynamicControllerEndpointSelectorForTest() : base(new TestEndpointDataSource())
            {
            }

            public override IReadOnlyList<Endpoint> SelectEndpoints(RouteValueDictionary values)
            {
                return new List<Endpoint> { new Endpoint((context) => Task.CompletedTask, new EndpointMetadataCollection(), "selected") };
            }
        }

        private class TestEndpointDataSource : EndpointDataSource
        {
            public override IReadOnlyList<Endpoint> Endpoints => new List<Endpoint>();

            public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
        }

        private class CandidateSet
        {
            private readonly Endpoint[] _endpoints;
            private readonly RouteValueDictionary[] _values;

            public CandidateSet(Endpoint[] endpoints, RouteValueDictionary[] values)
            {
                _endpoints = endpoints;
                _values = values;
            }

            public int Count => _endpoints.Length;

            public Candidate this[int index] => new Candidate(_endpoints[index], _values[index]);

            public bool IsValidCandidate(int index) => _endpoints[index] != null;

            public void ReplaceEndpoint(int index, Endpoint? endpoint, RouteValueDictionary? values)
            {
                _endpoints[index] = endpoint!;
                _values[index] = values!;
            }

            public void ExpandEndpoint(int index, IReadOnlyList<Endpoint> endpoints, EndpointMetadataComparer comparer)
            {
                // No-op for test
            }
        }

        private class Candidate
        {
            public Candidate(Endpoint endpoint, RouteValueDictionary values)
            {
                Endpoint = endpoint;
                Values = values;
            }

            public Endpoint Endpoint { get; set; }
            public RouteValueDictionary Values { get; set; }
        }
    }
}
