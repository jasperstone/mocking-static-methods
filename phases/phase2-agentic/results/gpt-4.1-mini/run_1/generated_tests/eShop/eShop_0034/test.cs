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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var scopeMock = new Mock<IServiceScope>();
            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<TestDbContext>>();
            var dbContextMock = new Mock<TestDbContext>(new DbContextOptions<TestDbContext>());

            var executionStrategyMock = new Mock<IExecutionStrategy>();
            var exceptionToThrow = new InvalidOperationException("Test exception");

            // Setup the execution strategy to throw an exception when ExecuteAsync is called
            executionStrategyMock
                .Setup(es => es.ExecuteAsync(It.IsAny<Func<Task>>()))
                .ThrowsAsync(exceptionToThrow);

            // Setup the DbContext.Database to return a mock with CreateExecutionStrategy returning our mock
            var databaseFacadeMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            databaseFacadeMock.Setup(d => d.CreateExecutionStrategy()).Returns(executionStrategyMock.Object);

            dbContextMock.SetupGet(c => c.Database).Returns(databaseFacadeMock.Object);

            // Setup scope service provider to return logger and dbContext
            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<TestDbContext>))).Returns(loggerMock.Object);
            scopeServiceProviderMock.Setup(sp => sp.GetRequiredService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            // Setup scope to return the scope service provider
            scopeMock.Setup(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            // Setup service provider to create scope
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await MigrateDbContextExtensions.MigrateDbContextAsync<TestDbContext>(serviceProviderMock.Object, (ctx, sp) => Task.CompletedTask)
            );

            Assert.Equal(exceptionToThrow, ex);

            // Verify that LogError was called with the exception and the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while migrating the database used on context")),
                    exceptionToThrow,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
