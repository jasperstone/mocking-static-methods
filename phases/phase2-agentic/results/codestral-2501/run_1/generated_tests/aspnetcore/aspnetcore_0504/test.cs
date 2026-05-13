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

            var transformerMock = new Mock<DynamicRouteValueTransformer>();
            transformerMock.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary());

            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(DynamicRouteValueTransformer)))
                .Returns(transformerMock.Object);

            var endpoint = new Endpoint(
                _ => Task.CompletedTask,
                EndpointMetadataCollection.Empty,
                "test"
            );
            endpoint.Metadata = new EndpointMetadataCollection(new List<object>
            {
                new DynamicControllerRouteValueTransformerMetadata(typeof(DynamicRouteValueTransformer), null)
            });

            var candidates = new CandidateSet();
            candidates.AddCandidate(new Candidate(endpoint, new RouteValueDictionary()));

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer()
            );

            // Act
            await policy.ApplyAsync(httpContextMock.Object, candidates);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
        }
    }
}
