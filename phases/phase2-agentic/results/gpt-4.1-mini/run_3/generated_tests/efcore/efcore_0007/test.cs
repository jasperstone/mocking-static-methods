using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options) { }
        }

        private interface ITestContextService { }

        private class TestContextImplementation : TestContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions<TestContextImplementation> options) : base(options) { }
        }

        [Fact]
        public void AddDbContextPool_AddsServicesIncludingGetServiceCall()
        {
            var services = new ServiceCollection();

            // Act
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => options.UseInMemoryDatabase("TestDb"),
                poolSize: 2);

            var provider = services.BuildServiceProvider();

            // The service IScopedDbContextLease<TestContextImplementation> should be registered
            var lease = provider.GetService<IScopedDbContextLease<TestContextImplementation>>();
            Assert.Null(lease); // Because scoped, no scope created yet

            // Create scope to test scoped services
            using (var scope = provider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;

                // The TContextService registration uses GetRequiredService<IScopedDbContextLease<TContextImplementation>>().Context
                var contextService = scopedProvider.GetService<ITestContextService>();
                Assert.NotNull(contextService);
                Assert.IsAssignableFrom<TestContextImplementation>(contextService);

                // The line under test calls GetService<TContextService>() on IServiceProvider and casts to TContextImplementation
                var contextImpl = scopedProvider.GetService<TestContextImplementation>();
                Assert.NotNull(contextImpl);
                Assert.IsType<TestContextImplementation>(contextImpl);
            }
        }
    }
}
