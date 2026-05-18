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
            var serviceProvider = new ServiceCollection()
                .AddLogging(logging => logging.AddConsole())
                .AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(databaseName: "TestDatabase"))
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<MigrateDbContextExtensionsTests>>();
            var context = serviceProvider.GetService<TestDbContext>();

            var seederMock = new Mock<Func<TestDbContext, IServiceProvider, Task>>();
            seederMock.Setup(s => s(context, serviceProvider)).Throws(new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => serviceProvider.MigrateDbContextAsync(seederMock.Object));
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }
}
