using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_Should_LogError_When_ExceptionThrown()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<SampleDbContext>>();
            var contextMock = new Mock<SampleDbContext>();
            var databaseMock = new Mock<DatabaseFacade>();
            var strategyMock = new Mock<IExecutionStrategy>();

            // Setup scope creation
            servicesMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(scopeServicesMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService<ILogger<SampleDbContext>>()).Returns(loggerMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService<SampleDbContext>()).Returns(contextMock.Object);
            contextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(db => db.CreateExecutionStrategy()).Returns(strategyMock.Object);
            strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async (func) =>
                {
                    await func();
                    throw new InvalidOperationException("Migration failed");
                });

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await MigrateDbContextAsync(servicesMock.Object, (ctx, sp) => Task.CompletedTask);
            });

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.Is<Exception>(e => e == ex),
                "An error occurred while migrating the database used on context {DbContextName}", typeof(SampleDbContext).Name),
                Times.Once);
        }

        // Sample DbContext for testing
        public class SampleDbContext : DbContext
        {
            public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }
        }
    }
}
