using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace EfCore.Tests
{
    public class AddDbContextPoolTests
    {
        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
            public static bool Configured { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                base.OnConfiguring(optionsBuilder);
                Configured = true;
            }
        }

        [Fact]
        public void AddDbContextPool_CallsGetService()
        {
            var services = new ServiceCollection();
            services.AddScoped<ScopedService>();
            services.TryAddScoped<IScopedDbContextLease<TestDbContext>, ScopedDbContextLease<TestDbContext>>();
            services.TryAddSingleton<IDbContextPool<TestDbContext>, DbContextPool<TestDbContext>>();

            // Register the context pool
            services.AddDbContextPool<TestDbContext, TestDbContext>((sp, ob) =>
            {
                // Call to GetService on IServiceProvider
                var lease = sp.GetService<IScopedDbContextLease<TestDbContext>>();
                Assert.NotNull(lease);
                Assert.IsType<ScopedDbContextLease<TestDbContext>>(lease);
            });

            var provider = services.BuildServiceProvider();

            // Resolve the context pool to ensure registration
            var pool = provider.GetService<IDbContextPool<TestDbContext>>();
            Assert.NotNull(pool);
        }

        private class ScopedService { }

        private class ScopedDbContextLease<T> : IScopedDbContextLease<T> where T : DbContext
        {
            public T Context { get; }
            public ScopedDbContextLease(T context) => Context = context;
        }

        private interface IScopedDbContextLease<T> where T : DbContext
        {
            T Context { get; }
        }
    }
}
