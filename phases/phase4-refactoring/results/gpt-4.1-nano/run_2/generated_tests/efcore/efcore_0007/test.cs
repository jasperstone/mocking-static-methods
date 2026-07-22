using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

namespace EfCoreTests
{
    public class AddDbContextPoolTests
    {
        private class DummyContext : DbContext
        {
            public DummyContext(DbContextOptions options) : base(options) { }
        }

        [Fact]
        public void AddDbContextPool_CallsGetService()
        {
            var services = new ServiceCollection();

            // Register DummyContext with AddDbContextPool explicitly specifying type parameters
            services.AddDbContextPool<DummyContext, DummyContext>(null, 10);

            var serviceProvider = services.BuildServiceProvider();

            // Create a scope to simulate a real usage scenario
            using var scope = serviceProvider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            // Resolve DummyContext, which should trigger the GetService call inside AddDbContextPool
            var context = scopedProvider.GetService<DummyContext>();
            Assert.NotNull(context);
        }
    }
}
