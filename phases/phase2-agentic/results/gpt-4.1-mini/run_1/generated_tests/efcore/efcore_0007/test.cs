using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceProviderServiceExtensionsTests
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
        public void AddDbContextPool_AddsServicesAndResolvesContext()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with TContextService != TContextImplementation triggers the GetService call on line 347
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => options.UseInMemoryDatabase("TestDb"),
                poolSize: 2);

            var provider = services.BuildServiceProvider();

            // Resolve the scoped lease and context
            using (var scope = provider.CreateScope())
            {
                var lease = scope.ServiceProvider.GetRequiredService<IScopedDbContextLease<TestContextImplementation>>();
                Assert.NotNull(lease);
                Assert.NotNull(lease.Context);

                // Resolve TContextService (ITestContextService) - should be scoped and not null
                var contextService = scope.ServiceProvider.GetService<ITestContextService>();
                Assert.NotNull(contextService);

                // Resolve TContextImplementation - this triggers the call to GetService<TContextService>() inside the factory
                var contextImplementation = scope.ServiceProvider.GetService<TestContextImplementation>();
                Assert.NotNull(contextImplementation);

                // The contextImplementation should be the same instance as the context from the lease
                Assert.Same(lease.Context, contextImplementation);
            }
        }
    }
}
