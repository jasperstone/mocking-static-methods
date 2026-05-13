using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options) : base(options) { }
        }

        private class TestSeeder : IDbSeeder<TestDbContext>
        {
            public bool SeedCalled { get; private set; }

            public Task SeedAsync(TestDbContext context)
            {
                SeedCalled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void AddMigration_WithSeederType_RegistersScopedSeederAndCallsGetRequiredService()
        {
            var services = new ServiceCollection();

            // Add a dummy DbContext registration for DI
            services.AddDbContext<TestDbContext>(options => { });

            // Add a logger for TestDbContext to avoid missing service error
            services.AddLogging();

            // Add the migration with seeder type
            services.AddMigration<TestDbContext, TestSeeder>();

            var provider = services.BuildServiceProvider();

            // Resolve the scoped service provider to test the seeder resolution
            using var scope = provider.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            // The call on line 28 is inside AddMigration<TContext, TDbSeeder>:
            // It calls sp.GetRequiredService<IDbSeeder<TContext>>().SeedAsync(context)
            // We want to verify that the IDbSeeder<TestDbContext> is registered and can be resolved.

            var seeder = scopedProvider.GetRequiredService<IDbSeeder<TestDbContext>>();
            Assert.NotNull(seeder);
            Assert.IsType<TestSeeder>(seeder);
        }
    }
}
