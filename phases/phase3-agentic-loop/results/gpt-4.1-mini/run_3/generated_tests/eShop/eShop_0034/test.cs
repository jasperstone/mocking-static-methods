using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        }

        [Fact]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionThrown()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");

            var loggerMock = new Mock<ILogger<TestDbContext>>();
            var executionStrategyMock = new Mock<IExecutionStrategy>();
            executionStrategyMock
                .Setup(s => s.ExecuteAsync(It.IsAny<Func<Task>>()))
                .ThrowsAsync(exception);

            var databaseMock = new Mock<DatabaseFacade>(MockBehavior.Strict, new TestDbContext());
            databaseMock.Setup(d => d.CreateExecutionStrategy()).Returns(executionStrategyMock.Object);

            var dbContextMock = new Mock<TestDbContext>();
            dbContextMock.SetupGet(c => c.Database).Returns(databaseMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<TestDbContext>))).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestDbContext))).Returns(dbContextMock.Object);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.SetupGet(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(serviceScopeMock.Object);

            var rootServiceProviderMock = new Mock<IServiceProvider>();
            rootServiceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await MigrateDbContextExtensions.MigrateDbContextAsync<TestDbContext>(rootServiceProviderMock.Object, (ctx, sp) => Task.CompletedTask));

            Assert.Same(exception, ex);

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
