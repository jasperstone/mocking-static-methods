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
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var transformerMock = new Mock<DynamicRouteValueTransformer>();
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(DynamicRouteValueTransformer), null);
            var endpoint = new Endpoint((_) => Task.CompletedTask, EndpointMetadataCollection.Empty, "test");
            endpoint.Metadata = new EndpointMetadataCollection(new List<object> { transformerMetadata });

            var candidates = new CandidateSet();
            candidates.AddCandidate(new Candidate(endpoint, new RouteValueDictionary()));

            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(transformerMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
        }
    }
}
