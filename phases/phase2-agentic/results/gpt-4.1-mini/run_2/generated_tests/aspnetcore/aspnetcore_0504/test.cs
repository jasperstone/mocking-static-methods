using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing
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

            var transformerMock = new Mock<DynamicRouteValueTransformer>();
            transformerMock.SetupProperty(t => t.State, null);
            transformerMock.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary(new Dictionary<string, object> { { "key", "value" } }));
            transformerMock.Setup(t => t.FilterAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>(), It.IsAny<IReadOnlyList<Endpoint>>()))
                .ReturnsAsync(new List<Endpoint> { new Endpoint((context) => Task.CompletedTask, new EndpointMetadataCollection(), "test") });

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(TestTransformer)))
                .Returns(transformerMock.Object);

            httpContext.RequestServices = serviceProviderMock.Object;

            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(TestTransformer))
            {
                State = null
            };

            var endpoint = new Endpoint(
                (context) => Task.CompletedTask,
                new EndpointMetadataCollection(transformerMetadata),
                "test");

            var candidates = new CandidateSetForTest(new[] { endpoint });

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(TestTransformer)), Times.Once);
            Assert.NotNull(candidates[0].Values);
            Assert.Equal("value", candidates[0].Values["key"]);
        }

        private class TestTransformer : DynamicRouteValueTransformer
        {
            public override ValueTask<RouteValueDictionary?> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                throw new NotImplementedException();
            }
        }

        // Minimal CandidateSet implementation for testing
        private class CandidateSetForTest : CandidateSet
        {
            private readonly Endpoint[] _endpoints;
            private readonly RouteValueDictionary?[] _values;
            private readonly bool[] _validCandidates;

            public CandidateSetForTest(Endpoint[] endpoints) : base(endpoints.Length)
            {
                _endpoints = endpoints;
                _values = new RouteValueDictionary?[endpoints.Length];
                _validCandidates = new bool[endpoints.Length];
                for (int i = 0; i < endpoints.Length; i++)
                {
                    _validCandidates[i] = true;
                }
            }

            public override int Count => _endpoints.Length;

            public override Endpoint? this[int index]
            {
                get => _endpoints[index];
                set => throw new NotSupportedException();
            }

            public override RouteValueDictionary? GetValues(int index) => _values[index];

            public override void ReplaceEndpoint(int index, Endpoint? endpoint, RouteValueDictionary? values)
            {
                _values[index] = values;
            }

            public override void ExpandEndpoint(int index, IReadOnlyList<Endpoint> endpoints, EndpointMetadataComparer comparer)
            {
                // No-op for test
            }

            public override bool IsValidCandidate(int index) => _validCandidates[index];
        }

        // Minimal DynamicControllerEndpointSelectorCache for testing
        private class DynamicControllerEndpointSelectorCacheForTest : DynamicControllerEndpointSelectorCache
        {
            public override DynamicControllerEndpointSelector GetEndpointSelector(Endpoint endpoint)
            {
                return new DynamicControllerEndpointSelectorForTest();
            }
        }

        // Minimal DynamicControllerEndpointSelector for testing
        private class DynamicControllerEndpointSelectorForTest : DynamicControllerEndpointSelector
        {
            public DynamicControllerEndpointSelectorForTest() : base(new EndpointDataSourceForTest())
            {
            }

            public override IReadOnlyList<Endpoint> SelectEndpoints(RouteValueDictionary values)
            {
                return new List<Endpoint> { new Endpoint((ctx) => Task.CompletedTask, new EndpointMetadataCollection(), "selected") };
            }
        }

        // Minimal EndpointDataSource for testing
        private class EndpointDataSourceForTest : EndpointDataSource
        {
            public override IReadOnlyList<Endpoint> Endpoints => new List<Endpoint>();

            public override IChangeToken GetChangeToken() => NullChangeToken.Singleton;
        }
    }
}
