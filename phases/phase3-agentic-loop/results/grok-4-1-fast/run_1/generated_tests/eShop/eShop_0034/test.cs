using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger<FakeDbContext>>();
            var fakeContext = new FakeDbContext();

            services.AddSingleton<ILogger<FakeDbContext>>(loggerMock.Object);
            services.AddSingleton(fakeContext);

            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => serviceProvider.MigrateDbContextAsync<FakeDbContext>((_, __) => throw new InvalidOperationException("Migration failed")));

            // Assert that LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while migrating the database used on context FakeDbContext")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task MigrateDbContextAsync_WhenSuccessful_LogsInformationButNoError()
        {
            // Arrange
            var services = new ServiceCollection();
            var loggerMock = new Mock<ILogger<FakeDbContext>>();

            services.AddSingleton<ILogger<FakeDbContext>>(loggerMock.Object);
            services.AddSingleton<FakeDbContext>();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            await serviceProvider.MigrateDbContextAsync<FakeDbContext>((_, __) => Task.CompletedTask);

            // Assert no error was logged
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    public class FakeDbContext : DbContext
    {
        public FakeDbContext() { }
        public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder<FakeDbContext> optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase("FakeDb");

        public DbSet<object> FakeEntities { get; set; } = null!;
    }
}
