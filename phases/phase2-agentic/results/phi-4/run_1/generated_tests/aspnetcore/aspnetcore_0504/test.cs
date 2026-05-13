using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

public class DynamicControllerEndpointMatcherPolicyTests
{
    [Fact]
    public async Task ApplyAsync_WhenDynamicControllerRouteValueTransformerMetadataPresent_InvokesGetRequiredService()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var serviceProvider = new Mock<IServiceProvider>();
        var transformer = new Mock<DynamicRouteValueTransformer>();
        var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(transformer.Object.GetType(), null);

        serviceProvider.Setup(s => s.GetRequiredService(transformerMetadata.SelectorType))
            .Returns(transformer.Object);

        var candidates = new CandidateSet(new List<EndpointMetadata>
        {
            new EndpointMetadataCollection(new[] { transformerMetadata })
        }, new List<Endpoint>
        {
            new Endpoint("TestEndpoint", new RoutePattern(new RoutePatternParser().Parse("/test")), new RouteValueDictionary())
        }, new RouteValueDictionary());

        var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

        // Act
        await policy.ApplyAsync(httpContext, candidates);

        // Assert
        serviceProvider.Verify(s => s.GetRequiredService(transformerMetadata.SelectorType), Times.Once);
        transformer.Verify(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()), Times.Once);
    }
}
