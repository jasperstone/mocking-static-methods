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
                    throw new InvalidOperationException("Migration failed");
                });

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await MigrateDbContextAsync(servicesMock.Object, (ctx, sp) => Task.CompletedTask);
            });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while migrating the database used on context")),
                    It.Is<Exception>(ex => ex is InvalidOperationException),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => base.Database;
    }
}
