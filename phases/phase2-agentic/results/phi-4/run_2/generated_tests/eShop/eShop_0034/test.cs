using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class MigrateDbContextExtensionsTests
    {
        [Fact]
        public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<MockDbContext>>();
            var context = new MockDbContext();
            var seeder = (MockDbContext ctx, IServiceProvider sp) => Task.CompletedTask;

            serviceProvider
                .Setup(s => s.GetRequiredService<ILogger<MockDbContext>>())
                .Returns(logger.Object);

            serviceProvider
                .Setup(s => s.GetRequiredService<MockDbContext>())
                .Returns(context);

            // Act & Assert
            var exception = new Exception("Test exception");
            await Assert.ThrowsAsync<Exception>(() => MigrateDbContextExtensions.MigrateDbContextAsync(serviceProvider.Object, seeder));

            logger.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "An error occurred while migrating the database used on context {DbContextName}",
                    It.Is<Type>(t => t == typeof(MockDbContext))
                ),
                Times.Once
            );
        }

        private class MockDbContext : DbContext
        {
        }
    }
}
