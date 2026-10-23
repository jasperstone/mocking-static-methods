using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceProviderServiceExtensionsTests
    {
        private interface ITestContextService
        {
            string GetData();
        }

        private class TestContextImplementation : DbContext, ITestContextService
        {
            public TestContextImplementation(DbContextOptions options) : base(options) { }

            public string GetData() => "Hello";
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

            // Resolve the service of type ITestContextService
            var service = provider.GetRequiredService<ITestContextService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ITestContextService>(service);

            // Resolve the implementation type explicitly, which uses the service provider's GetService<TContextService> call internally
            var implementation = provider.GetRequiredService<TestContextImplementation>();
            Assert.NotNull(implementation);
            Assert.IsType<TestContextImplementation>(implementation);

            // The implementation instance should be the same as the service instance cast to implementation type
            Assert.Equal(service, implementation);
        }
    }
}
