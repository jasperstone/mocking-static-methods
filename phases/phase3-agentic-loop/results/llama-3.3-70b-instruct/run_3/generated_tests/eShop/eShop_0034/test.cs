using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddConsole());
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase("Test"));
            var serviceProvider = services.BuildServiceProvider();
            var loggerMock = new Mock<ILogger<TestDbContext>>();
            services.AddSingleton(typeof(ILogger<TestDbContext>), loggerMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => serviceProvider.MigrateDbContextAsync<TestDbContext>((context, sp) => throw new Exception("Test exception")));

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "An error occurred while migrating the database used on context {DbContextName}", It.IsAny<object[]>()), Times.Once);
        }

        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }
        }
    }
}
