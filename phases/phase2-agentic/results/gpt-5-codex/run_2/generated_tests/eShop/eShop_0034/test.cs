using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace eShop.Tests.Shared
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_LogsErrorAndRethrowsWhenSeederFails()
        {
            var seederException = new InvalidOperationException("Seeder failed");
            var testLogger = new TestLogger<TestDbContext>();

            var services = new ServiceCollection();
            services.AddSingleton<ILogger<TestDbContext>>(testLogger);
            services.AddDbContext<TestDbContext>(
                options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped);

            using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            var migrateMethod = GetMigrateDbContextAsyncMethod();
            Func<TestDbContext, IServiceProvider, Task> failingSeeder = async (_, _) =>
            {
                await Task.Yield();
                throw seederException;
            };

            Task Act() => (Task)migrateMethod.Invoke(null, new object[] { serviceProvider, failingSeeder })!;

            var thrown = await Assert.ThrowsAsync<InvalidOperationException>(Act);
            Assert.Same(seederException, thrown);

            var errorEntry = Assert.Single(testLogger.Entries.Where(entry => entry.LogLevel == LogLevel.Error));
            Assert.Same(seederException, errorEntry.Exception);
            Assert.Equal("An error occurred while migrating the database used on context TestDbContext", errorEntry.Message);
        }

        private static MethodInfo GetMigrateDbContextAsyncMethod()
        {
            var assembly = typeof(IDbSeeder<>).Assembly;
            var extensionsType = assembly.GetType("Microsoft.AspNetCore.Hosting.MigrateDbContextExtensions", throwOnError: true)!;
            var methodDefinition = extensionsType.GetMethod("MigrateDbContextAsync", BindingFlags.NonPublic | BindingFlags.Static)!;
            return methodDefinition.MakeGenericMethod(typeof(TestDbContext));
        }

        private sealed class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options)
            {
            }
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose()
                {
                }
            }

            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, eventId, exception, formatter(state, exception)));
            }
        }

        private record LogEntry(LogLevel LogLevel, EventId EventId, Exception? Exception, string Message);
    }
}
