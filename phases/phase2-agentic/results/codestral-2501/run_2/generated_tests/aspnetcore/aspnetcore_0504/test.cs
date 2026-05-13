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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

            var transformerType = typeof(MockDynamicRouteValueTransformer);
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(transformerType, null);
            var endpoint = new Endpoint(_ => Task.CompletedTask, EndpointMetadataCollection.Empty, "test endpoint");
            endpoint.Metadata = new EndpointMetadataCollection(new List<object> { transformerMetadata });

            var candidates = new CandidateSet();
            candidates.AddCandidate(new Candidate(endpoint, new RouteValueDictionary(), 0));

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new Mock<DynamicControllerEndpointSelectorCache>().Object,
                new Mock<EndpointMetadataComparer>().Object);

            // Act
            await policy.ApplyAsync(httpContextMock.Object, candidates);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(transformerType), Times.Once);
        }

        private class MockDynamicRouteValueTransformer : DynamicRouteValueTransformer
        {
            public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
            }
        }
    }
}
