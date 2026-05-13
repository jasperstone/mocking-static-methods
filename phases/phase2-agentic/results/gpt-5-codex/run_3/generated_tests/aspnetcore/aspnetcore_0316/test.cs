using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class IdentityApiEndpointRouteBuilderExtensionsTests
    {
        [Fact]
        public void MapIdentityApi_ThrowsWhenBearerTokenOptionsMissing()
        {
            var serviceProvider = new RecordingServiceProvider(new Dictionary<Type, object>
            {
                { typeof(TimeProvider), TimeProvider.System }
            });
            var endpointRouteBuilder = new TestEndpointRouteBuilder(serviceProvider);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpointRouteBuilder));

            Assert.Contains("No service for type", exception.Message);
            Assert.Contains($"'{typeof(IOptionsMonitor<BearerTokenOptions>).FullName}'", exception.Message);
        }

        [Fact]
        public void MapIdentityApi_RequestsBearerTokenOptionsFromServiceProvider()
        {
            var serviceProvider = new RecordingServiceProvider(new Dictionary<Type, object>
            {
                { typeof(TimeProvider), TimeProvider.System }
            });
            var endpointRouteBuilder = new TestEndpointRouteBuilder(serviceProvider);

            Assert.Throws<InvalidOperationException>(() =>
                IdentityApiEndpointRouteBuilderExtensions.MapIdentityApi<TestUser>(endpointRouteBuilder));

            Assert.Equal(new[]
            {
                typeof(TimeProvider),
                typeof(IOptionsMonitor<BearerTokenOptions>)
            }, serviceProvider.RequestedTypes);
        }

        private sealed class TestUser
        {
        }

        private sealed class RecordingServiceProvider : IServiceProvider
        {
            private readonly IDictionary<Type, object> _services;

            public RecordingServiceProvider(IDictionary<Type, object> services)
            {
                _services = services;
            }

            public List<Type> RequestedTypes { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedTypes.Add(serviceType);
                return _services.TryGetValue(serviceType, out var instance) ? instance : null;
            }
        }

        private sealed class TestEndpointRouteBuilder : IEndpointRouteBuilder
        {
            public TestEndpointRouteBuilder(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
                DataSources = new List<EndpointDataSource>();
            }

            public IServiceProvider ServiceProvider { get; }

            public ICollection<EndpointDataSource> DataSources { get; }

            public IApplicationBuilder CreateApplicationBuilder()
                => throw new NotImplementedException();
        }
    }
}
