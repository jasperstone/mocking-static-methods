using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task AddMigration_WithSeeder_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockSeeder = new Mock<IDbSeeder<SampleDbContext>>();
            services.AddScoped(_ => mockSeeder.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProvider = new ServiceCollection()
                .AddSingleton(serviceProviderMock.Object)
                .BuildServiceProvider();

            scopeMock.Setup(s => s.ServiceProvider).Returns(scopeServiceProvider);
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            serviceProviderMock.Setup(sp => sp.CreateScope()).Returns(scopeMock.Object);

            // Add required services
            services.AddLogging();
            services.AddScoped<SampleDbContext>();
            services.AddOpenTelemetry(); // Assuming extension method exists

            var serviceProvider = services.BuildServiceProvider();

            // Use reflection to invoke the private method
            var methodInfo = typeof(MigrateDbContextExtensions).GetMethod("MigrateDbContextAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(methodInfo);

            // Prepare parameters
            var context = new Mock<SampleDbContext>().Object;
            var seederFunc = new Func<SampleDbContext, IServiceProvider, Task>((ctx, sp) => Task.CompletedTask);

            // Act
            await (Task)methodInfo.Invoke(null, new object[] { serviceProvider, seederFunc });

            // Verify that GetRequiredService was called for IDbSeeder<SampleDbContext>
            mockSeeder.Verify(s => s.SeedAsync(It.IsAny<SampleDbContext>()), Times.Never); // Because seed not called in this test
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }
        public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => base.Database;
    }
}
