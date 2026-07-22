using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceProviderServiceExtensionsTests
    {
        private interface ITestContextService { }
        private class TestContextImplementation : DbContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions options) : base(options) { }
        }

        [Fact]
        public void AddDbContextPool_AddsServices_And_ResolvesContextImplementationFromServiceProvider()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with different service and implementation types triggers the GetService call on IServiceProvider
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => { }, // no-op optionsAction
                poolSize: 2);

            var provider = services.BuildServiceProvider();

            // Resolve the scoped DbContext service (ITestContextService)
            using var scope = provider.CreateScope();
            var contextService = scope.ServiceProvider.GetService<ITestContextService>();
            Assert.NotNull(contextService);
            Assert.IsType<TestContextImplementation>(contextService);

            // Resolve the implementation type via the factory that calls GetService<TContextService>()
            var contextImplementation = scope.ServiceProvider.GetService<TestContextImplementation>();
            Assert.NotNull(contextImplementation);
            Assert.IsType<TestContextImplementation>(contextImplementation);
        }
    }
}
