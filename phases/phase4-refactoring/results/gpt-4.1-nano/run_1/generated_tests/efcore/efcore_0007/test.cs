using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

namespace EfCoreTests
{
    public class AddDbContextPoolTests
    {
        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        }

        [Fact]
        public void AddDbContextPool_RegistersServicesAndCallsGetService()
        {
            var services = new ServiceCollection();

            // Arrange: Add a mock or a real service provider
            services.AddScoped<TestDbContext>();
            var serviceProvider = services.BuildServiceProvider();

            // Act: Call the method under test
            services.AddDbContextPool<TestDbContext, TestDbContext>((sp, ob) =>
            {
                ob.UseInMemoryDatabase("TestDb");
            });

            var provider = services.BuildServiceProvider();

            // Assert: Check that the services are registered
            var context = provider.GetService<TestDbContext>();
            Assert.NotNull(context);

            // Check that GetService is called on IServiceProvider
            var scopedLease = provider.GetService<IScopedDbContextLease<TestDbContext>>();
            Assert.NotNull(scopedLease);
            Assert.IsType<ScopedDbContextLease<TestDbContext>>(scopedLease);
        }
    }
}
