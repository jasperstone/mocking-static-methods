using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests;

public class DynamicControllerEndpointMatcherPolicyTests
{
    [Fact]
    public async Task GetRequiredService_ThrowsInvalidOperationException_WhenTransformerHasState()
    {
        // Arrange
        var services = new ServiceCollection();
        var badTransformer = new BadStateTransformer();
        services.AddSingleton<DynamicRouteValueTransformer>(badTransformer);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var endpoint = CreateTransformerEndpoint(typeof(BadStateTransformer));

        var candidates = new CandidateSet(1);
        candidates.SetValidCandidate(0, endpoint, new RouteValueDictionary());

        var policy = CreatePolicy();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => policy.ApplyAsync(httpContext, candidates));

        Assert.Contains("must be registered as transient when using state", exception.Message);
    }

    [Fact]
    public async Task GetRequiredService_Succeeds_WhenTransformerHasNoState()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<DynamicRouteValueTransformer>(provider => new GoodTransformer());
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var endpoint = CreateTransformerEndpoint(typeof(GoodTransformer));

        var candidates = new CandidateSet(1);
        candidates.SetValidCandidate(0, endpoint, new RouteValueDictionary());

        var policy = CreatePolicy();

        // Act
        await policy.ApplyAsync(httpContext, candidates);

        // Assert - test completes without exception
        Assert.True(true);
    }

    private static DynamicControllerEndpointMatcherPolicy CreatePolicy()
    {
        return new DynamicControllerEndpointMatcherPolicy(
            new DynamicControllerEndpointSelectorCache(),
            new DefaultEndpointMetadataComparer());
    }

    private static Endpoint CreateTransformerEndpoint(Type transformerType)
    {
        var metadata = new DynamicControllerRouteValueTransformerMetadata(transformerType, "test-state");
        var endpoint = new Endpoint(
            context => Task.CompletedTask,
            new EndpointMetadataCollection(new[] { metadata }),
            "test");

        return endpoint;
    }

    private class BadStateTransformer : DynamicRouteValueTransformer
    {
        public BadStateTransformer()
        {
            State = new object(); // Has state already
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
        }
    }

    private class GoodTransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
        }
    }

    private class DefaultEndpointMetadataComparer : IComparer<EndpointMetadataCollection>
    {
        public int Compare(EndpointMetadataCollection? x, EndpointMetadataCollection? y)
        {
            return 0;
        }
    }
}
