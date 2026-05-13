using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Tests
{
    [TestClass]
    public class MigrateDbContextExtensionsTests
    {
        [TestMethod]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddConsole());
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("TestDb"));
            services.AddSingleton<ILogger<TestDbContext>>(new Mock<ILogger<TestDbContext>>().Object);
            var serviceProvider = services.BuildServiceProvider();

            var loggerMock = new Mock<ILogger<TestDbContext>>();
            services.AddSingleton<ILogger<TestDbContext>>(loggerMock.Object);

            var seeder = new Func<TestDbContext, IServiceProvider, Task>((context, sp) => throw new Exception("Test exception"));

            // Act
            try
            {
                await serviceProvider.MigrateDbContextAsync(seeder);
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred while migrating the database used on context {DbContextName}", typeof(TestDbContext).Name), Times.Once);
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }
}
