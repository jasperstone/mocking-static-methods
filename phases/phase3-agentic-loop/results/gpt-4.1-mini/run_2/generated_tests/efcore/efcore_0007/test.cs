using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private interface ITestContextService
        {
            string GetData();
        }

        private class TestContextImplementation : DbContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions options) : base(options)
            {
            }

            public string GetData() => "Hello";
        }

        [Fact]
        public void AddDbContextPool_AddsScopedService_WithGetServiceCall()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with TContextService != TContextImplementation triggers the GetService call line
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => options.UseInMemoryDatabase("TestDb"),
                poolSize: 10);

            var provider = services.BuildServiceProvider();

            // Resolve the scoped service TContextService
            using (var scope = provider.CreateScope())
            {
                var contextService = scope.ServiceProvider.GetRequiredService<ITestContextService>();
                Assert.NotNull(contextService);
                Assert.IsType<TestContextImplementation>(contextService);

                // Also resolve TContextImplementation via the scoped factory that calls GetService<TContextService>()
                var contextImpl = scope.ServiceProvider.GetRequiredService<TestContextImplementation>();
                Assert.NotNull(contextImpl);
                Assert.Same(contextService, contextImpl);
            }
        }
    }
}
