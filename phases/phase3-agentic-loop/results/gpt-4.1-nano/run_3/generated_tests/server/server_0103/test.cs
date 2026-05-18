using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.Models.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace NotificationRepositoryTests
{
    public class NotificationRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<DbContext> _dbContextMock;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();

            _serviceScopeFactoryMock
                .Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(_serviceScopeMock.Object);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_CallsCreateAsyncScope()
        {
            // Arrange
            var taskId = Guid.NewGuid();

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() }
            };

            var notificationStatuses = new List<NotificationStatus>
            {
                new NotificationStatus { NotificationId = notifications[0].Id, DeletedDate = null },
                new NotificationStatus { NotificationId = notifications[1].Id, DeletedDate = null }
            };

            var mockDbSetNotifications = new Mock<DbSet<Notification>>();
            mockDbSetNotifications.As<IQueryable<Notification>>().Setup(m => m.Provider).Returns(notifications.AsQueryable().Provider);
            mockDbSetNotifications.As<IQueryable<Notification>>().Setup(m => m.Expression).Returns(notifications.AsQueryable().Expression);
            mockDbSetNotifications.As<IQueryable<Notification>>().Setup(m => m.ElementType).Returns(notifications.AsQueryable().ElementType);
            mockDbSetNotifications.As<IQueryable<Notification>>().Setup(m => m.GetEnumerator()).Returns(notifications.AsQueryable().GetEnumerator());

            var mockDbSetStatuses = new Mock<DbSet<NotificationStatus>>();
            mockDbSetStatuses.As<IQueryable<NotificationStatus>>().Setup(m => m.Provider).Returns(notificationStatuses.AsQueryable().Provider);
            mockDbSetStatuses.As<IQueryable<NotificationStatus>>().Setup(m => m.Expression).Returns(notificationStatuses.AsQueryable().Expression);
            mockDbSetStatuses.As<IQueryable<NotificationStatus>>().Setup(m => m.ElementType).Returns(notificationStatuses.AsQueryable().ElementType);
            mockDbSetStatuses.As<IQueryable<NotificationStatus>>().Setup(m => m.GetEnumerator()).Returns(notificationStatuses.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<DbContext>();
            mockDbContext.Setup(c => c.Notifications).Returns(mockDbSetNotifications.Object);
            mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockDbSetStatuses.Object);
            mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Create a derived class to override GetDatabaseContext
            var repo = new TestNotificationRepository(_serviceScopeFactoryMock.Object, null, mockDbContext.Object);

            // Act
            var result = await repo.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            Assert.NotNull(result);
        }

        // Helper class to override GetDatabaseContext
        private class TestNotificationRepository : NotificationRepository
        {
            private readonly DbContext _dbContext;

            public TestNotificationRepository(IServiceScopeFactory factory, IMapper mapper, DbContext dbContext)
                : base(factory, mapper)
            {
                _dbContext = dbContext;
            }

            protected override DbContext GetDatabaseContext(IServiceScope scope)
            {
                return _dbContext;
            }
        }
    }
}
