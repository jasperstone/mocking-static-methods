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
        public async Task ApplyAsync_GetRequiredServiceCalled()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProvider = new ServiceCollection().AddScoped<TestDynamicRouteValueTransformer>().BuildServiceProvider();
            httpContext.RequestServices = serviceProvider;

            var transformerMetadata = new DynamicControllerRouteValueTransformerMetadata(typeof(TestDynamicRouteValueTransformer), null);
            var endpoint = new Endpoint(httpContext => Task.CompletedTask, new List<object> { transformerMetadata }, "/test");

            var candidates = new CandidateSet(new[] { new Candidate(endpoint, new RouteValueDictionary()) });
            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(httpContext, candidates);

            // Assert
            var transformer = (TestDynamicRouteValueTransformer)httpContext.RequestServices.GetRequiredService(typeof(TestDynamicRouteValueTransformer));
            Assert.NotNull(transformer);
        }

        private class TestDynamicRouteValueTransformer : DynamicRouteValueTransformer
        {
            public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
            {
                return new ValueTask<RouteValueDictionary>(values);
            }
        }
    }
}
