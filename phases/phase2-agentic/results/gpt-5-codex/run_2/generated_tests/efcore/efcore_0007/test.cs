using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContextPool_RegistersImplementationUsingServiceProviderServiceExtensions()
        {
            var services = new ServiceCollection();
            services.AddDbContextPool<IFakeContextService, FakeContext>((sp, ob) => { });

            var descriptor = Assert.Single(
                services.Where(d => d.ServiceType == typeof(FakeContext) && d.ImplementationFactory is not null));

            var context = new FakeContext(new DbContextOptions<FakeContext>());
            var provider = new SpyServiceProvider(typeof(IFakeContextService), context);

            var result = descriptor.ImplementationFactory!(provider);

            Assert.Same(context, result);
            Assert.Equal(new[] { typeof(IFakeContextService) }, provider.RequestedServices);
        }

        private interface IFakeContextService
        {
        }

        private sealed class FakeContext : DbContext, IFakeContextService
        {
            public FakeContext(DbContextOptions<FakeContext> options)
                : base(options)
            {
            }
        }

        private sealed class SpyServiceProvider : IServiceProvider
        {
            private readonly Type _serviceType;
            private readonly object? _serviceInstance;

            public SpyServiceProvider(Type serviceType, object? serviceInstance)
            {
                _serviceType = serviceType;
                _serviceInstance = serviceInstance;
            }

            public List<Type> RequestedServices { get; } = new();

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);
                return serviceType == _serviceType ? _serviceInstance : null;
            }
        }
    }
}
