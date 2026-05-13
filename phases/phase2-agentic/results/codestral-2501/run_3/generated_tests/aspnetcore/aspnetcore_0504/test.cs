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

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_WithTransformerMetadata_ShouldCallGetRequiredService()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var transformerMock = new Mock<DynamicRouteValueTransformer>();
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(DynamicRouteValueTransformer), null);
            var endpoint = new Endpoint((_) => Task.CompletedTask, EndpointMetadataCollection.Empty, "test");
            endpoint.Metadata = new EndpointMetadataCollection(new List<object> { transformerMetadata });

            var candidates = new CandidateSet();
            candidates.AddCandidate(new Candidate(endpoint, new RouteValueDictionary(), 0));

            var selectorCacheMock = new Mock<DynamicControllerEndpointSelectorCache>();
            var comparerMock = new Mock<EndpointMetadataComparer>();
            var policy = new DynamicControllerEndpointMatcherPolicy(selectorCacheMock.Object, comparerMock.Object);

            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(transformerMock.Object);
            transformerMock.Setup(t => t.TransformAsync(httpContextMock.Object, It.IsAny<RouteValueDictionary>())).ReturnsAsync(new RouteValueDictionary());

            // Act
            await policy.ApplyAsync(httpContextMock.Object, candidates);

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
        }
    }
}
