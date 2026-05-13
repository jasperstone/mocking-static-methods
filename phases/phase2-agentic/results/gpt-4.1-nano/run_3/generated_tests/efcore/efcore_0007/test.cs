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
        public void AddDbContext_ShouldRegisterContextWithCorrectLifetime()
        {
            var services = new ServiceCollection();

            services.AddDbContext<DummyContext>(options => { });

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetService<DummyContext>();
            Assert.NotNull(context);
        }

        [Fact]
        public void AddDbContext_WithImplementation_ShouldRegisterImplementation()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ITestService, DummyContext>((sp, ob) => { });

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<ITestService>();
            Assert.NotNull(context);
            Assert.IsType<DummyContext>(context);
        }

        [Fact]
        public void AddDbContext_ShouldCallGetServiceOnServiceProvider()
        {
            var services = new ServiceCollection();

            var mockLease = new Mock<IScopedDbContextLease<DummyContext>>();
            mockLease.Setup(l => l.Context).Returns(new DummyContext(new DbContextOptions<DummyContext>()));

            services.TryAddScoped(_ => mockLease.Object);
            services.TryAddScoped<DummyContext>(sp => sp.GetRequiredService<IScopedDbContextLease<DummyContext>>().Context);

            var provider = services.BuildServiceProvider();

            var context = provider.GetService<DummyContext>();
            Assert.NotNull(context);
        }
    }
}
