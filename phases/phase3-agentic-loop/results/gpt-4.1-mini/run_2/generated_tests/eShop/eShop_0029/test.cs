using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MigrateDbContextExtensionsTests
{
    public class MigrationExtensionsTests
    {
        [Fact]
        public void AddMigration_WithSeederType_RegistersScopedService()
        {
            var services = new ServiceCollection();

            var returnedServices = Microsoft.AspNetCore.Hosting.MigrateDbContextExtensions.AddMigration<TestDbContext, TestDbSeeder>(services);

            Assert.Same(services, returnedServices);

            var provider = services.BuildServiceProvider();

            var seeder = provider.GetService<IDbSeeder<TestDbContext>>();
            Assert.NotNull(seeder);
            Assert.IsType<TestDbSeeder>(seeder);
        }

        [Fact]
        public async Task SeederDelegate_CallsGetRequiredServiceAndSeedAsync()
        {
            var services = new ServiceCollection();

            var seederMock = new Mock<IDbSeeder<TestDbContext>>();
            seederMock.Setup(s => s.SeedAsync(It.IsAny<TestDbContext>())).Returns(Task.CompletedTask).Verifiable();

            services.AddScoped(_ => seederMock.Object);

            var provider = services.BuildServiceProvider();

            Func<TestDbContext, IServiceProvider, Task> seederDelegate = (context, sp) =>
                sp.GetRequiredService<IDbSeeder<TestDbContext>>().SeedAsync(context);

            var context = new TestDbContext();

            await seederDelegate(context, provider);

            seederMock.Verify();
        }

        private class TestDbContext : DbContext
        {
        }

        private class TestDbSeeder : IDbSeeder<TestDbContext>
        {
            public Task SeedAsync(TestDbContext context) => Task.CompletedTask;
        }
    }
}
