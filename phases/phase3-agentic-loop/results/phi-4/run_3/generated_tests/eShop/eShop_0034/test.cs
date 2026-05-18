    using System;
    using System.Threading;
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
            public async Task MigrateDbContextAsync_LogsError_WhenExceptionOccurs()
            {
                // Arrange
                var serviceProviderMock = new Mock<IServiceProvider>();
                var loggerMock = new Mock<ILogger<MockDbContext>>();
                var contextMock = new Mock<MockDbContext>();
                var seederMock = new Mock<Func<MockDbContext, IServiceProvider, Task>>();

                serviceProviderMock
                    .Setup(sp => sp.GetRequiredService<ILogger<MockDbContext>>())
                    .Returns(loggerMock.Object);

                serviceProviderMock
                    .Setup(sp => sp.GetRequiredService<MockDbContext>())
                    .Returns(contextMock.Object);

                // Act & Assert
                var exception = new Exception("Test exception");
                await Assert.ThrowsAsync<Exception>(() => MigrateDbContextExtensions.MigrateDbContextAsync(serviceProviderMock.Object, seederMock.Object));

                loggerMock.Verify(
                    l => l.LogError(
                        It.IsAny<Exception>(),
                        "An error occurred while migrating the database used on context {DbContextName}",
                        It.IsAny<Type>()
                    ),
                    Times.Once
                );
            }

            private class MockDbContext : DbContext
            {
                public MockDbContext(DbContextOptions<MockDbContext> options) : base(options) { }
            }
        }
    }
    