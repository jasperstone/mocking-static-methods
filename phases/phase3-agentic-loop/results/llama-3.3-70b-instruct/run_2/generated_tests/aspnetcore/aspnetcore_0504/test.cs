using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_GetRequiredService_Called()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var requestServices = new Mock<IServiceProvider>();
            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(MockDynamicRouteValueTransformer), null);
            var endpoint = new Endpoint(
                async context =>
                {
                    await Task.CompletedTask;
                },
                new List<object> { transformerMetadata },
                "/test");

            var candidates = new CandidateSet(new[] { new Candidate(endpoint, new RouteValueDictionary()) });
            var selectorCache = new DynamicControllerEndpointSelectorCache();
            var comparer = new EndpointMetadataComparer();
            var policy = new DynamicControllerEndpointMatcherPolicy(selectorCache, comparer);

            httpContext.SetupGet(c => c.RequestServices).Returns(requestServices.Object);
            requestServices.Setup(s => s.GetRequiredService(It.IsAny<Type>())).Returns(new MockDynamicRouteValueTransformer());

            // Act
            await policy.ApplyAsync(httpContext.Object, candidates);

            // Assert
            requestServices.Verify(s => s.GetRequiredService(It.IsAny<Type>()), Times.Once);
        }

        private class MockDynamicRouteValueTransformer : DynamicRouteValueTransformer
        {
            public override Task<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return Task.FromResult(values);
            }
        }
    }
}
