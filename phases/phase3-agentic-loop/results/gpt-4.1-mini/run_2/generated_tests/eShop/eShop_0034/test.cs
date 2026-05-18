using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        private class DummyContext : DbContext
        {
            public DummyContext(DbContextOptions<DummyContext> options) : base(options) { }
        }

        [Fact]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DummyContext>>();
            var dbContextMock = new Mock<DummyContext>(new DbContextOptions<DummyContext>());

            var executionStrategyMock = new Mock<IExecutionStrategy>();
            var exception = new InvalidOperationException("Test exception");

            executionStrategyMock
                .Setup(es => es.ExecuteAsync(It.IsAny<Func<Task>>()))
                .ThrowsAsync(exception);

            var databaseFacadeMock = new Mock<DatabaseFacade>(dbContextMock.Object);
            databaseFacadeMock
                .Setup(db => db.CreateExecutionStrategy())
                .Returns(executionStrategyMock.Object);

            dbContextMock
                .SetupGet(c => c.Database)
                .Returns(databaseFacadeMock.Object);

            var scopeServiceProviderMock = new Mock<IServiceProvider>();
            scopeServiceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ILogger<DummyContext>)))
                .Returns(loggerMock.Object);
            scopeServiceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(DummyContext)))
                .Returns(dbContextMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.SetupGet(s => s.ServiceProvider).Returns(scopeServiceProviderMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.CreateScope())
                .Returns(scopeMock.Object);

            // Act & Assert
            var seeder = new Func<DummyContext, IServiceProvider, Task>((ctx, sp) => Task.CompletedTask);

            var migrateDbContextAsyncMethod = typeof(Microsoft.AspNetCore.Hosting.MigrateDbContextExtensions)
                .GetMethod("MigrateDbContextAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .MakeGenericMethod(typeof(DummyContext));

            var task = (Task)migrateDbContextAsyncMethod.Invoke(null, new object[] { serviceProviderMock.Object, seeder });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => task);

            Assert.Equal(exception, ex);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while migrating the database used on context")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
