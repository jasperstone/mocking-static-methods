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
        public async Task MigrateDbContextAsync_Should_Call_LogError_When_ExceptionThrown()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServicesMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<SampleDbContext>>();
            var contextMock = new Mock<SampleDbContext>();
            var databaseMock = new Mock<DatabaseFacade>();
            var strategyMock = new Mock<IExecutionStrategy>();

            // Setup the scope creation
            servicesMock.Setup(s => s.CreateScope()).Returns(scopeMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(scopeServicesMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService<ILogger<SampleDbContext>>()).Returns(loggerMock.Object);
            scopeServicesMock.Setup(s => s.GetRequiredService<SampleDbContext>()).Returns(contextMock.Object);
            contextMock.Setup(c => c.Database).Returns(databaseMock.Object);
            databaseMock.Setup(db => db.CreateExecutionStrategy()).Returns(strategyMock.Object);
            strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
                .Returns<Func<Task>>(async (func) => await func());

            // Act
            await MigrateDbContextAsync_TestHelper(servicesMock.Object, loggerMock.Object, contextMock.Object, true);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), 
                                "An error occurred while migrating the database used on context {DbContextName}", 
                                typeof(SampleDbContext).Name),
                Times.Once);
        }

        private async Task MigrateDbContextAsync_TestHelper(
            IServiceProvider services,
            ILogger<SampleDbContext> logger,
            SampleDbContext context,
            bool throwException)
        {
            var ex = new Exception("Test exception");
            if (throwException)
            {
                // Setup to throw exception during execution
                var strategyMock = context.Database.CreateExecutionStrategy();
                strategyMock.Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
                    .Returns<Func<Task>>(async (func) => { throw ex; });
            }

            // Call the private method via reflection or make it internal for testing
            await MigrateDbContextExtensions.MigrateDbContextAsync<SampleDbContext>(
                services,
                async (ctx, sp) =>
                {
                    if (throwException)
                        throw ex;
                    await Task.CompletedTask;
                });
        }

        // Sample DbContext for testing
        public class SampleDbContext : Microsoft.EntityFrameworkCore.DbContext
        {
            public Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database => base.Database;
        }
    }
}
