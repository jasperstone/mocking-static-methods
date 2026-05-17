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
        public async Task ApplyAsync_CallsGetRequiredServiceOnRequestServices_WithTransformerSelectorType()
        {
            // Arrange
            var selectorCache = new DynamicControllerEndpointSelectorCache();
            var comparer = new EndpointMetadataComparer();

            var policy = new DynamicControllerEndpointMatcherPolicy(selectorCache, comparer);

            var httpContext = new DefaultHttpContext();

            var serviceProviderMock = new Mock<IServiceProvider>();
            var transformerMock = new Mock<DynamicRouteValueTransformer>();

            var transformerType = typeof(TestDynamicRouteValueTransformer);
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(transformerType, state: null);

            var endpointMetadata = new List<object> { transformerMetadata };

            var endpointMock = new Mock<Endpoint>(null, new EndpointMetadataCollection(endpointMetadata), "TestEndpoint");
            var routeValues = new RouteValueDictionary();

            // Setup candidate set mock
            var candidatesMock = new Mock<CandidateSet>(1);
            candidatesMock.Setup(c => c.Count).Returns(1);
            candidatesMock.Setup(c => c.IsValidCandidate(0)).Returns(true);
            candidatesMock.SetupGet(c => c[0]).Returns(new CandidateState(endpointMock.Object, routeValues));
            candidatesMock.Setup(c => c.ReplaceEndpoint(It.IsAny<int>(), It.IsAny<Endpoint?>(), It.IsAny<RouteValueDictionary?>()));
            candidatesMock.Setup(c => c.ExpandEndpoint(It.IsAny<int>(), It.IsAny<IReadOnlyList<Endpoint>>(), It.IsAny<EndpointMetadataComparer>()));

            // Setup service provider to return transformerMock when GetRequiredService is called with transformerType
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(transformerType))
                .Returns(transformerMock.Object);

            httpContext.RequestServices = serviceProviderMock.Object;

            transformerMock.SetupGet(t => t.State).Returns((object?)null);
            transformerMock.SetupSet(t => t.State = It.IsAny<object?>());
            transformerMock.Setup(t => t.TransformAsync(httpContext, routeValues))
                .ReturnsAsync(new RouteValueDictionary { { "key", "value" } });
            transformerMock.Setup(t => t.FilterAsync(httpContext, It.IsAny<RouteValueDictionary>(), It.IsAny<IReadOnlyList<Endpoint>>()))
                .ReturnsAsync(new List<Endpoint> { endpointMock.Object });

            var dataSourceIdMetadata = new ControllerEndpointDataSourceIdMetadata(1);
            endpointMock.Setup(e => e.Metadata.GetMetadata<ControllerEndpointDataSourceIdMetadata>())
                .Returns(dataSourceIdMetadata);

            var selectorMock = new Mock<DynamicControllerEndpointSelector>(MockBehavior.Strict, null as EndpointDataSource);
            selectorMock.Setup(s => s.SelectEndpoints(It.IsAny<RouteValueDictionary>()))
                .Returns(new List<Endpoint> { endpointMock.Object });

            var dataSourceMock = new Mock<EndpointDataSource>();
            selectorCache.AddDataSource(dataSourceMock.Object, 1);

            var selectorCacheType = typeof(DynamicControllerEndpointSelectorCache);
            var selectorCacheField = selectorCacheType.GetField("_endpointSelectorCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var endpointSelectorCache = (System.Collections.Concurrent.ConcurrentDictionary<int, DynamicControllerEndpointSelector>)selectorCacheField.GetValue(selectorCache)!;
            endpointSelectorCache[1] = selectorMock.Object;

            // Act
            await policy.ApplyAsync(httpContext, candidatesMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(transformerType), Times.Once);
            transformerMock.VerifySet(t => t.State = transformerMetadata.State, Times.Once);
            transformerMock.Verify(t => t.TransformAsync(httpContext, routeValues), Times.Once);
            transformerMock.Verify(t => t.FilterAsync(httpContext, It.IsAny<RouteValueDictionary>(), It.IsAny<IReadOnlyList<Endpoint>>()), Times.Once);
        }

        private class CandidateState : Microsoft.AspNetCore.Routing.CandidateState
        {
            public CandidateState(Endpoint endpoint, RouteValueDictionary values)
            {
                Endpoint = endpoint;
                Values = values;
            }

            public override Endpoint Endpoint { get; }
            public override RouteValueDictionary? Values { get; set; }
        }
    }
}
