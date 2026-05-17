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
        private class TestDbContext : DbContext { }

        private class TestDbSeeder : IDbSeeder<TestDbContext>
        {
            public bool SeedCalled { get; private set; }

            public Task SeedAsync(TestDbContext context)
            {
                SeedCalled = true;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void AddMigration_WithSeeder_CallsGetRequiredServiceOnServiceProvider()
        {
            var services = new ServiceCollection();

            // Add scoped IDbSeeder<TestDbContext> to services
            services.AddScoped<IDbSeeder<TestDbContext>, TestDbSeeder>();

            // Call AddMigration<TContext, TDbSeeder> which internally calls GetRequiredService on IServiceProvider
            var result = services.AddMigration<TestDbContext, TestDbSeeder>();

            Assert.NotNull(result);
            Assert.Contains(result, d => d.ServiceType == typeof(IDbSeeder<TestDbContext>));
        }

        [Fact]
        public async Task MigrationHostedService_StartAsync_InvokesMigrateDbContextAsync()
        {
            var services = new ServiceCollection();

            var seederMock = new Mock<Func<TestDbContext, IServiceProvider, Task>>();
            seederMock.Setup(s => s(It.IsAny<TestDbContext>(), It.IsAny<IServiceProvider>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();

            var loggerMock = new Mock<ILogger<TestDbContext>>();
            var dbContextMock = new Mock<TestDbContext>();
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            var executionStrategyMock = new Mock<IExecutionStrategy>();

            executionStrategyMock.Setup(es => es.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(func => func());

            databaseMock.Setup(db => db.CreateExecutionStrategy()).Returns(executionStrategyMock.Object);
            dbContextMock.SetupGet(c => c.Database).Returns(databaseMock.Object);

            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<TestDbContext>))).Returns(loggerMock.Object);
            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            scopeMock.SetupGet(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(scopeMock.Object);

            var hostedService = new MigrationHostedService<TestDbContext>(serviceProviderMock.Object, seederMock.Object);

            await hostedService.StartAsync(default);

            seederMock.Verify(s => s(It.IsAny<TestDbContext>(), It.IsAny<IServiceProvider>()), Times.Once);
        }
    }
}
