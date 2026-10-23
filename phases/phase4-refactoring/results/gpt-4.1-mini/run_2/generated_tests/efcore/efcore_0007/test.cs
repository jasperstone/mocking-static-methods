using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private interface ITestContextService
        {
        }

        private class TestContextImplementation : DbContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions options) : base(options)
            {
            }
        }

        [Fact]
        public void AddDbContextPool_AddsServices_And_ResolvesContextImplementationFromServiceProvider()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with different service and implementation types triggers the GetService call on IServiceProvider
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => { }, // no-op optionsAction
                poolSize: 2);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the scoped DbContext service (ITestContextService)
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;

                // Resolve the service of type ITestContextService
                var contextService = scopedProvider.GetRequiredService<ITestContextService>();
                Assert.NotNull(contextService);
                Assert.IsType<TestContextImplementation>(contextService);

                // Resolve the service of type TestContextImplementation via the factory that calls GetService<TContextService>()
                var contextImplementation = scopedProvider.GetService<TestContextImplementation>();
                Assert.NotNull(contextImplementation);
                Assert.IsType<TestContextImplementation>(contextImplementation);

                // The two resolved instances should be the same instance (scoped)
                Assert.Same(contextService, contextImplementation);
            }
        }
    }
}
