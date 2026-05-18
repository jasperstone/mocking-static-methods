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
        public async Task ApplyAsync_WhenDynamicControllerRouteValueTransformerMetadataPresent_ReturnsExpectedEndpoints()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new Mock<IServiceProvider>();
            var transformer = new Mock<DynamicRouteValueTransformer>();
            transformer.Setup(t => t.TransformAsync(It.IsAny<HttpContext>(), It.IsAny<RouteValueDictionary>()))
                .ReturnsAsync(new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });

            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(
                typeof(DynamicRouteValueTransformer),
                null);

            serviceProvider.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)))
                .Returns(transformer.Object);

            var endpoint = new Endpoint(
                metadata: new List<object> { transformerMetadata },
                routePattern: "{controller=Home}/{action=Index}");

            var candidates = new CandidateSet(
                new List<Candidate>
                {
                    new Candidate(endpoint, new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } })
                });

            var policy = new DynamicControllerEndpointMatcherPolicy(
                new DynamicControllerEndpointSelectorCache(),
                new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            Assert.Single(candidates.Endpoints);
            Assert.NotNull(candidates.Values[0]);
        }
    }
}
