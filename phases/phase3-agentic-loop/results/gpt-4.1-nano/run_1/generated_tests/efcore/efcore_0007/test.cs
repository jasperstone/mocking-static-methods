using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace EfCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private class DummyContext : DbContext
        {
            public DummyContext(DbContextOptions options) : base(options) { }
        }

        private class Service
        {
            public object Context { get; set; }
        }

        [Fact]
        public void AddDbContextPool_Should_Call_GetService_For_TContextService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a dummy service that returns a context
            services.TryAddScoped<IScopedDbContextLease<DummyContext>>(sp =>
            {
                var lease = new ScopedDbContextLease<DummyContext>();
                lease.Context = new DummyContext(new DbContextOptions<DummyContext>());
                return lease;
            });

            // Act
            services.AddDbContextPool<DummyContext, DummyContext>((sp, ob) => { }, 10);

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Resolve the service
            var service = provider.GetService<DummyContext>();
            Assert.NotNull(service);
            Assert.IsType<DummyContext>(service);
        }
    }

    // Dummy implementations for interfaces used in the extension method
    public interface IScopedDbContextLease<T> where T : DbContext
    {
        T Context { get; }
    }

    public class ScopedDbContextLease<T> : IScopedDbContextLease<T> where T : DbContext
    {
        public T Context { get; set; }
    }
}
