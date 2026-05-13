using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Entities;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_CallsCreateAsyncScope()
        {
            // Arrange
            var mockScopeFactory = new Mock<IServiceScopeFactory>();
            var mockScope = new Mock<IServiceScope>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new Mock<DbContext>();

            mockScopeFactory.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(DbContext))).Returns(mockDbContext.Object);

            var repository = new NotificationRepository(mockScopeFactory.Object, null);

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid() }
            };

            var notificationStatuses = new List<NotificationStatus>
            {
                new NotificationStatus { NotificationId = notifications[0].Id, UserId = notifications[0].UserId.Value }
            };

            mockDbContext.Setup(db => db.Set<Notification>().Where(It.IsAny<Func<Notification, bool>>()))
                .Returns((Func<Notification, bool> predicate) => notifications.AsQueryable().Where(predicate));
            mockDbContext.Setup(db => db.Set<NotificationStatus>().Where(It.IsAny<Func<NotificationStatus, bool>>()))
                .Returns((Func<NotificationStatus, bool> predicate) => notificationStatuses.AsQueryable().Where(predicate));

            // Act
            await repository.MarkNotificationsAsDeletedByTask(notifications[0].TaskId);

            // Assert
            mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
