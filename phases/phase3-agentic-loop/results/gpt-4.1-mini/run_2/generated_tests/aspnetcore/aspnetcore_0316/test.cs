using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Routing;

public class IdentityApiEndpointRouteBuilderExtensionsTests
{
    private class TestUser { }

    private class DummyEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public IServiceProvider ServiceProvider { get; }

        public ICollection<EndpointDataSource> DataSources { get; } = new List<EndpointDataSource>();

        public DummyEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IEndpointConventionBuilder MapGroup(string prefix)
        {
            // Return a dummy IEndpointConventionBuilder that supports MapPost
            return new DummyEndpointConventionBuilder();
        }

        private class DummyEndpointConventionBuilder : IEndpointConventionBuilder
        {
            public void Add(Action<EndpointBuilder> convention) { }
        }
    }

    [Fact]
    public void MapIdentityApi_ThrowsArgumentNullException_WhenEndpointsIsNull()
    {
        IEndpointRouteBuilder? endpoints = null;
        Assert.Throws<ArgumentNullException>(() => IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpoints!));
    }

    [Fact]
    public void MapIdentityApi_ResolvesRequiredServicesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register required services for MapIdentityApi
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOptionsMonitor<BearerTokenOptions>>(new OptionsMonitorStub());
        services.AddSingleton<IEmailSender<TestUser>>(new EmailSenderStub());
        services.AddSingleton<LinkGenerator>(new LinkGeneratorStub());

        var serviceProvider = services.BuildServiceProvider();

        var endpointRouteBuilder = new DummyEndpointRouteBuilder(serviceProvider);

        // Act
        var result = IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpointRouteBuilder);

        // Assert
        Assert.NotNull(result);
    }

    // Stub classes for required services

    private class OptionsMonitorStub : IOptionsMonitor<BearerTokenOptions>
    {
        public BearerTokenOptions CurrentValue => new BearerTokenOptions();

        public BearerTokenOptions Get(string name) => new BearerTokenOptions();

        public IDisposable OnChange(Action<BearerTokenOptions, string> listener) => null!;
    }

    private class EmailSenderStub : IEmailSender<TestUser>
    {
        public System.Threading.Tasks.Task SendEmailAsync(TestUser user, string subject, string htmlMessage)
        {
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

    private class LinkGeneratorStub : LinkGenerator
    {
        public override string? GetPathByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary? values = null, RouteValueDictionary? ambientValues = null, FragmentString fragment = default)
        {
            return "/";
        }

        public override string? GetPathByRouteValues(HttpContext httpContext, string routeName, RouteValueDictionary? values, RouteValueDictionary? ambientValues = null, FragmentString fragment = default)
        {
            return "/";
        }
    }
}
