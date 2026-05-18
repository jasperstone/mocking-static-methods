using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            
            var loggerMock = new Mock<ILogger<MockDbContext>>();
            bool logErrorCalled = false;
            
            loggerMock
                .Setup(x => x.LogError(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()
                ))
                .Callback<Exception, string, object[]>((ex, msg, args) =>
                {
                    logErrorCalled = true;
                    Assert.Contains("MockDbContext", msg);
                    Assert.Contains("An error occurred while migrating the database", msg);
                });

            services.AddSingleton<ILogger<MockDbContext>>(loggerMock.Object);
            services.AddScoped<MockDbContext>();

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var scopeServices = scope.ServiceProvider;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => scopeServices.MigrateDbContextAsync(async (context, sp) =>
                {
                    throw new InvalidOperationException("Migration failed");
                }));

            Assert.Equal("Migration failed", exception.Message);
            Assert.True(logErrorCalled, "LogError should have been called");
        }

        private class MockDbContext : DbContext
        {
            public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }

            public override void Dispose() { }
            public override ValueTask DisposeAsync() => default;
        }
    }
}
