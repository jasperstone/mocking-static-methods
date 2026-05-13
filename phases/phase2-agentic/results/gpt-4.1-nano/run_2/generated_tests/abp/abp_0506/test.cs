using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.EntityFrameworkCore.Migrations;

namespace Volo.Abp.EntityFrameworkCore.Tests
{
    public class EfCoreDatabaseMigrationEventHandlerBaseTests
    {
        private class TestHandler : EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>
        {
            public List<string> LogMessages { get; } = new List<string>();
            public List<Exception> LoggedExceptions { get; } = new List<Exception>();
            public List<LogLevel> LoggedLevels { get; } = new List<LogLevel>();

            public TestHandler(
                string databaseName,
                ICurrentTenant currentTenant,
                IUnitOfWorkManager unitOfWorkManager,
                ITenantStore tenantStore,
                IAbpDistributedLock distributedLock,
                IDistributedEventBus distributedEventBus,
                ILoggerFactory loggerFactory)
                : base(databaseName, currentTenant, unitOfWorkManager, tenantStore, distributedLock, distributedEventBus, loggerFactory)
            {
            }

            protected override Task<bool> MigrateDatabaseSchemaAsync(Guid? tenantId)
            {
                return Task.FromResult(true);
            }

            protected override Task SeedAsync(Guid? tenantId)
            {
                return Task.CompletedTask;
            }

            public void CaptureLog(string message, LogLevel level, Exception ex = null)
            {
                LogMessages.Add(message);
                LoggedLevels.Add(level);
                if (ex != null)
                {
                    LoggedExceptions.Add(ex);
                }
            }
        }

        private class MockDbContext : IDisposable
        {
            public Mock<IDbContextProvider<MockDbContext>> DbContextProviderMock { get; } = new Mock<IDbContextProvider<MockDbContext>>();
            public Mock<IDbContextProvider<MockDbContext>>.ObjectMock => DbContextProviderMock.Object;

            public void Dispose() { }
        }

        [Fact]
        public async Task HandleErrorOnApplyDatabaseMigrationAsync_Should_LogErrorAndException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>())
                .Returns(loggerMock.Object);

            var handler = new TestHandler(
                "TestDb",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                Mock.Of<IDistributedEventBus>(),
                loggerFactoryMock.Object);

            var exception = new Exception("Test exception");
            var eventData = new TenantCreatedEto { Id = Guid.NewGuid(), Name = "TestTenant" };

            // Act
            await handler.HandleErrorTenantCreatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant created event. Canceling the operation.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Contains("Could not perform tenant created event. Canceling the operation.", handler.LogMessages);
            Assert.Contains(exception, handler.LoggedExceptions);
        }

        [Fact]
        public async Task HandleErrorTenantConnectionStringUpdatedAsync_Should_LogErrorAndException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>())
                .Returns(loggerMock.Object);

            var handler = new TestHandler(
                "TestDb",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                Mock.Of<IDistributedEventBus>(),
                loggerFactoryMock.Object);

            var exception = new Exception("Test exception");
            var eventData = new TenantConnectionStringUpdatedEto
            {
                Id = Guid.NewGuid(),
                Name = "TestTenant",
                ConnectionStringName = "Default",
                NewValue = "SomeConnectionString"
            };

            // Act
            await handler.HandleErrorTenantConnectionStringUpdatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant connection string updated event. Re-queueing the operation.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Contains("Could not perform tenant connection string updated event. Re-queueing the operation.", handler.LogMessages);
            Assert.Contains(exception, handler.LoggedExceptions);
        }

        [Fact]
        public async Task HandleErrorOnApplyDatabaseMigrationAsync_Should_LogErrorAndException_WhenMaxTryCountExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<EfCoreDatabaseMigrationEventHandlerBase<MockDbContext>>())
                .Returns(loggerMock.Object);

            var handler = new TestHandler(
                "TestDb",
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IUnitOfWorkManager>(),
                Mock.Of<ITenantStore>(),
                Mock.Of<IAbpDistributedLock>(),
                Mock.Of<IDistributedEventBus>(),
                loggerFactoryMock.Object);

            handler.MaxEventTryCount = 1; // Set max try count to 1 for test

            var exception = new Exception("Test exception");
            var eventData = new TenantCreatedEto { Id = Guid.NewGuid(), Name = "TestTenant" };

            // Simulate try count exceeding max
            handler.SetEventTryCount(eventData, 2);

            // Act
            await handler.HandleErrorTenantCreatedAsync(eventData, exception);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not perform tenant created event. Canceling the operation.")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.Contains("Could not perform tenant created event. Canceling the operation.", handler.LogMessages);
            Assert.Contains(exception, handler.LoggedExceptions);
        }
    }
}
