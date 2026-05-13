using System;
using System.Threading.Tasks;
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

        [Fact]
        public async Task MigrateDbContextAsync_LogsErrorOnException()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<TestDbContext>>();
            var dbContextMock = new Mock<TestDbContext>(new DbContextOptions<TestDbContext>());

            var executionStrategyMock = new Mock<IExecutionStrategy>();
            var exception = new InvalidOperationException("Test exception");

            // Setup scope and service provider
            servicesMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);
            scopeMock.SetupGet(s => s.ServiceProvider).Returns(scopeServicesMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService(typeof(ILogger<TestDbContext>))).Returns(loggerMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            // Setup execution strategy to throw exception when ExecuteAsync is called
            executionStrategyMock.Setup(es => es.ExecuteAsync(It.IsAny<Func<Task>>()))
                .ThrowsAsync(exception);

            // Setup DbContext.Database.CreateExecutionStrategy to return our mock
            var databaseMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            databaseMock.Setup(d => d.CreateExecutionStrategy()).Returns(executionStrategyMock.Object);
            dbContextMock.SetupGet(c => c.Database).Returns(databaseMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await MigrateDbContextExtensions.MigrateDbContextAsync<TestDbContext>(servicesMock.Object, (ctx, sp) => Task.CompletedTask));

            Assert.Equal(exception, ex);

            // Verify LogError was called with the exception and correct message
            loggerMock.Verify(
                x => x.LogError(
                    exception,
                    "An error occurred while migrating the database used on context {DbContextName}",
                    typeof(TestDbContext).Name),
                Times.Once);
        }
    }
}
