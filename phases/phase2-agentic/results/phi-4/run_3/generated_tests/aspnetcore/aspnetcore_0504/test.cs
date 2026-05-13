using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        public async Task ApplyAsync_WhenDynamicControllerRouteValueTransformerMetadataPresent_UsesGetRequiredService()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new Mock<IServiceProvider>();
            var transformer = new Mock<DynamicRouteValueTransformer>();
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata
            {
                SelectorType = typeof(DynamicRouteValueTransformer)
            };

            serviceProvider
                .Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)))
                .Returns(transformer.Object);

            var endpoint = new Endpoint(
                metadata: new List<object> { transformerMetadata },
                routePattern: null,
                httpMethod: null,
                routeValues: null,
                display: null,
                order: 0,
                dataTokens: null,
                behaviors: null,
                constraints: null,
                displayName: null,
                httpMethods: null,
                metadataCacheKey: null,
                routePatternString: null,
                routePatternSyntax: null,
                routeTemplate: null,
                routeValuesCacheKey: null,
                routeValuesString: null,
                template: null,
                templateString: null,
                templateMetadata: null,
                templateMatch: null,
                templateParameters: null,
                templateSyntax: null,
                uri: null,
                uriString: null,
                userProvided: null,
                virtualPath: null,
                virtualPathString: null,
                virtualPathData: null,
                virtualPathFactory: null,
                virtualPathRoot: null,
                virtualPathRootString: null,
                virtualPathProvider: null);

            var candidate = new Candidate(endpoint, new RouteValueDictionary());
            var candidates = new CandidateSet(new[] { candidate });

            httpContext.RequestServices = serviceProvider.Object;

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            serviceProvider.Verify(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
            transformer.Verify(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()), Times.Once);
        }
    }
}
