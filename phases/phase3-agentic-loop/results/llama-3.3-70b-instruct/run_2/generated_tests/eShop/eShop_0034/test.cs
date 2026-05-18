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
            services.AddDbContext<TestDbContext>(options => options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
            services.AddTransient<ILogger<TestDbContext>, TestLogger>();
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<TestDbContext>>();

            var seeder = new Func<TestDbContext, IServiceProvider, Task>((context, services) => throw new Exception("Test exception"));

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => serviceProvider.MigrateDbContextAsync(seeder));
            Assert.True(((TestLogger)logger).ErrorLogged);
        }

        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }
        }

        private class TestLogger : ILogger<TestDbContext>
        {
            public bool ErrorLogged { get; private set; }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    ErrorLogged = true;
                }
            }
        }
    }
}
