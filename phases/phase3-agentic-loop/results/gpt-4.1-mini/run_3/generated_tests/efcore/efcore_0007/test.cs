using System;
using Microsoft.EntityFrameworkCore;
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
            public TestContextImplementation(DbContextOptions<TestContextImplementation> options)
                : base(options)
            {
            }

            public string GetData() => "Hello";
        }

        [Fact]
        public void AddDbContextPool_AddsServices_And_ResolvesImplementationFromService()
        {
            var services = new ServiceCollection();

            // AddDbContextPool with TContextService != TContextImplementation triggers the GetService call on IServiceProvider
            services.AddDbContextPool<ITestContextService, TestContextImplementation>(
                (sp, options) => options.UseInMemoryDatabase("TestDb"),
                poolSize: 2);

            var provider = services.BuildServiceProvider();

            // Resolve the service of type ITestContextService
            var service = provider.GetService<ITestContextService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ITestContextService>(service);

            // Resolve the implementation type from the scoped factory that calls GetService<TContextService>()
            using (var scope = provider.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;

                // The scoped service of TContextImplementation is registered as a scoped service that calls GetService<TContextService>()
                var impl = scopedProvider.GetService<TestContextImplementation>();
                Assert.NotNull(impl);
                Assert.IsType<TestContextImplementation>(impl);

                // The implementation instance should be the same as the service instance resolved as ITestContextService
                var serviceFromImpl = scopedProvider.GetService<ITestContextService>();
                Assert.Same(serviceFromImpl, impl);
            }
        }
    }
}
