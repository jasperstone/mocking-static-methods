using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private class TestContext : DbContext { }
        private interface ITestContextService { }
        private class TestContextImplementation : DbContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions options) : base(options) { }
        }

        [Fact]
        public void AddDbContextPool_AddsServicesAndCallsGetService()
        {
            var services = new ServiceCollection();

            // Use AddDbContextPool with TContextService != TContextImplementation to trigger the GetService call line
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => options.UseInMemoryDatabase("TestDb"),
                poolSize: 2);

            var provider = services.BuildServiceProvider();

            // Resolve the TContextService scoped service
            using (var scope = provider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;

                // The scoped service TContextService should be resolvable
                var contextService = scopedProvider.GetService<ITestContextService>();
                Assert.NotNull(contextService);
                Assert.IsType<TestContextImplementation>(contextService);

                // The scoped service TContextImplementation should be resolvable and is created by calling GetService<TContextService>()
                var contextImplementation = scopedProvider.GetService<TestContextImplementation>();
                Assert.NotNull(contextImplementation);
                Assert.Same(contextService, contextImplementation);
            }
        }
    }
}
