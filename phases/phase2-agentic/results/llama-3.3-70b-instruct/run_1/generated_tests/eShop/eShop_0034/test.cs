using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [TestMethod]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddConsole());
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            var serviceProvider = services.BuildServiceProvider();

            var loggerMock = new Mock<ILogger<TestDbContext>>();
            var contextMock = new Mock<TestDbContext>();

            contextMock.Setup(c => c.Database.CreateExecutionStrategy()).Returns(new ExecutionStrategy());

            var seeder = new Func<TestDbContext, IServiceProvider, Task>((context, services) => throw new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => serviceProvider.MigrateDbContextAsync(seeder));
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred while migrating the database used on context TestDbContext"), Times.Once);
        }
    }

    public class TestDbContext : DbContext
    {
    }

    public class ExecutionStrategy : IExecutionStrategy
    {
        public bool RetriesOnFailure => false;

        public async Task ExecuteAsync(Func<Task> operation)
        {
            await operation();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
        {
            return await operation();
        }
    }
}
