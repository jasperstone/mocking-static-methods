using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DynamicControllerEndpointMatcherPolicyTests
{
    [Fact]
    public async Task ApplyAsync_WithTransformerMetadata_ShouldCallGetRequiredService()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockRequestServices = new Mock<IServiceProvider>();
        var mockTransformer = new Mock<DynamicRouteValueTransformer>();
        var mockCandidateSet = new Mock<CandidateSet>();
        var mockEndpoint = new Mock<Endpoint>();
        var mockMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(DynamicRouteValueTransformer), null);

        mockHttpContext.Setup(h => h.RequestServices).Returns(mockRequestServices.Object);
        mockRequestServices.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(mockTransformer.Object);
        mockCandidateSet.Setup(c => c.Count).Returns(1);
        mockCandidateSet.Setup(c => c.IsValidCandidate(0)).Returns(true);
        mockCandidateSet.Setup(c => c[0]).Returns(new Candidate(mockEndpoint.Object, new RouteValueDictionary()));
        mockEndpoint.Setup(e => e.Metadata.GetMetadata<DynamicControllerRouteValueTransformerMetadata>()).Returns(mockMetadata);

        var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

        // Act
        await policy.ApplyAsync(mockHttpContext.Object, mockCandidateSet.Object);

        // Assert
        mockRequestServices.Verify(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
    }
}
