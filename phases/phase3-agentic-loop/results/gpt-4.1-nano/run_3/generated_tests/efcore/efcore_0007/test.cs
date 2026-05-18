using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;

namespace EfCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private class DummyContext : DbContext
        {
            public DummyContext(DbContextOptions options) : base(options) { }
        }

        [Fact]
        public void AddDbContext_ShouldRegisterContextWithOptions()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>(opts => { });

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<DummyContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void AddDbContext_ShouldRegisterScopedService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>();

            var provider = services.BuildServiceProvider();

            var scope = provider.CreateScope();
            var context1 = scope.ServiceProvider.GetService<DummyContext>();
            var context2 = scope.ServiceProvider.GetService<DummyContext>();
            Assert.NotNull(context1);
            Assert.NotNull(context2);
            Assert.NotSame(context1, context2);
        }

        [Fact]
        public void AddDbContext_WithInterface_ShouldRegisterService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>();

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<DummyContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void AddDbContext_ShouldCallGetService()
        {
            var services = new ServiceCollection();

            services.AddScoped<ScopedService>();
            services.AddScoped(sp => (DummyContext)sp.GetService<DummyContext>());

            var provider = services.BuildServiceProvider();

            var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetService<DummyContext>();
            Assert.NotNull(context);
        }

        private class ScopedService
        {
            public DummyContext Context { get; }
            public ScopedService(DummyContext context)
            {
                Context = context;
            }
        }
    }
}
