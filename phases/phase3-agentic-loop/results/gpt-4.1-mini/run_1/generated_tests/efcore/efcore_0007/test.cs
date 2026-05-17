using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
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
        public void AddDbContextPool_AddsScopedServiceThatCallsGetServiceOnIServiceProvider()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with TContextService != TContextImplementation triggers the GetService call in the factory
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => { });

            var provider = services.BuildServiceProvider();

            // Resolve the scoped service that calls GetService<TContextService>() internally
            using var scope = provider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            // The service registered with the factory that calls GetService<TContextService>() is of type TestContextImplementation
            var resolved = scopedProvider.GetService<TestContextImplementation>();

            // The resolved instance should not be null
            Assert.NotNull(resolved);

            // The resolved instance should be assignable to ITestContextService
            Assert.IsAssignableFrom<ITestContextService>(resolved);
        }
    }
}
