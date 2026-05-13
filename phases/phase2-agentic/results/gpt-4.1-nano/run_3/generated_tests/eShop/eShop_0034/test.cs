using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_Should_LogError_When_ExceptionOccurs()
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
                    return true;
                });

            var ex = new InvalidOperationException("Test exception");
            var activityMock = new Mock<Activity>();
            var activitySourceMock = new Mock<ActivitySource>();
            // Patch ActivitySource.StartActivity to return a disposable activity
            // Since ActivitySource is static, we can't mock directly, so we will just test the LogError call

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await servicesMock.Object.MigrateDbContextAsync<SampleDbContext>(async (ctx, sp) =>
                {
                    throw ex;
                });
            });

            // Assert
            loggerMock.Verify(
                l => l.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(SampleDbContext).Name),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => base.Database;
    }
}
