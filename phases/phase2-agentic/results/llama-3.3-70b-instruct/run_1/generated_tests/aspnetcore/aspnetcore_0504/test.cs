using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
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
        public async Task ApplyAsync_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;

            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(MockDynamicRouteValueTransformer), null);
            var endpoint = new Endpoint(new List<EndpointMetadata> { transformerMetadata }, null, null);

            var candidates = new CandidateSet(new[] { new Candidate(endpoint, new RouteValueDictionary()) });
            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            var transformer = (MockDynamicRouteValueTransformer)httpContext.RequestServices.GetRequiredService(typeof(MockDynamicRouteValueTransformer));
            Assert.NotNull(transformer);
        }

        private class MockDynamicRouteValueTransformer : DynamicRouteValueTransformer
        {
            public override Task<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                throw new NotImplementedException();
            }
        }
    }
}
