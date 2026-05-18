using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests;

public class DynamicControllerEndpointMatcherPolicyTests
{
    [Fact]
    public async Task GetRequiredService_ThrowsInvalidOperationException_WhenServiceNotRegistered()
    {
        // Arrange
        var endpoint = CreateTransformerEndpoint(typeof(TestTransformer));
        var candidates = new CandidateSet(1);
        candidates.Add(new Candidate(endpoint, new RouteValueDictionary(), 0));
        candidates[0].IsValid = true;

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

        var policy = new DynamicControllerEndpointMatcherPolicy(null!, null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => policy.ApplyAsync(httpContext, candidates));
        
        Assert.Contains("Unable to resolve service for type", exception.Message);
    }

    [Fact]
    public async Task GetRequiredService_Succeeds_WhenServiceRegistered()
    {
        // Arrange
        var endpoint = CreateTransformerEndpoint(typeof(TestTransformer));
        var candidates = new CandidateSet(1);
        candidates.Add(new Candidate(endpoint, new RouteValueDictionary(), 0));
        candidates[0].IsValid = true;

        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddSingleton<TestTransformer>(new TestTransformer());
        httpContext.RequestServices = services.BuildServiceProvider();

        var policy = new DynamicControllerEndpointMatcherPolicy(null!, null!);

        // Act
        await policy.ApplyAsync(httpContext, candidates);

        // Assert - no exception thrown
        Assert.True(true);
    }

    [Fact]
    public async Task GetRequiredService_ThrowsInvalidOperationException_WhenTransformerStateNotNull()
    {
        // Arrange
        var endpoint = CreateTransformerEndpoint(typeof(TestTransformerWithState));
        var candidates = new CandidateSet(1);
        candidates.Add(new Candidate(endpoint, new RouteValueDictionary(), 0));
        candidates[0].IsValid = true;

        var httpContext = new DefaultHttpContext();
        var services = new ServiceCollection();
        var transformer = new TestTransformerWithState { State = new object() };
        services.AddSingleton(typeof(TestTransformerWithState), transformer);
        httpContext.RequestServices = services.BuildServiceProvider();

        var policy = new DynamicControllerEndpointMatcherPolicy(null!, null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => policy.ApplyAsync(httpContext, candidates));
        
        Assert.Contains("State should be null", exception.Message);
    }

    private static Endpoint CreateTransformerEndpoint(Type transformerType)
    {
        var metadata = new DynamicControllerRouteValueTransformerMetadata(transformerType, null);
        var endpoint = new Endpoint(null!, new EndpointMetadataCollection(metadata), "test");
        return endpoint;
    }

    private class TestTransformer : DynamicRouteValueTransformer
    {
        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
        }
    }

    private class TestTransformerWithState : DynamicRouteValueTransformer
    {
        public new object? State { get; set; }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            return new ValueTask<RouteValueDictionary>(new RouteValueDictionary());
        }
    }
}
