using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EfCore.Tests
{
    public class EntityFrameworkServiceCollectionExtensionsTests
    {
        private class DummyContext : DbContext
        {
            public DummyContext(DbContextOptions options) : base(options) { }
        }

        private class DummyService { }

        [Fact]
        public void AddDbContext_ShouldRegisterContextWithOptions()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>(options => options.UseInMemoryDatabase("TestDb"));

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<DummyContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void AddDbContext_ShouldRegisterScopedServices()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>(options => options.UseInMemoryDatabase("TestDb"));

            var provider = services.BuildServiceProvider();

            var scope = provider.CreateScope();
            var context1 = scope.ServiceProvider.GetService<DummyContext>();
            var context2 = scope.ServiceProvider.GetService<DummyContext>();
            Assert.NotNull(context1);
            Assert.NotNull(context2);
            Assert.NotSame(context1, context2);
        }

        [Fact]
        public void AddDbContext_WithServiceType_ShouldRegisterService()
        {
            var services = new ServiceCollection();

            services.AddDbContext<IDbContext, DummyContext>(options => options.UseInMemoryDatabase("TestDb"));

            var provider = services.BuildServiceProvider();

            var service = provider.GetService<IDbContext>();
            Assert.NotNull(service);
            Assert.IsType<DummyContext>(service);
        }

        [Fact]
        public void AddDbContext_ShouldCallGetServiceOnProvider()
        {
            var services = new ServiceCollection();

            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(p => p.GetService(typeof(DummyContext))).Returns(new DummyContext(new DbContextOptions<DummyContext>()));

            services.AddScoped(_ => mockProvider.Object);

            services.AddDbContext<DummyContext>(options => options.UseInMemoryDatabase("TestDb"));

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<DummyContext>();
            Assert.NotNull(context);
        }
    }
}
