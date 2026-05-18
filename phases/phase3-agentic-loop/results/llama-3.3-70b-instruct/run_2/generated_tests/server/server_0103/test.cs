using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Core.NotificationCenter.Entities;
using Bit.Core.NotificationCenter.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ValidTaskId_ReturnsUserIds()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var mapperMock = new Mock<IMapper>();
            var notificationRepository = new NotificationRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() }
            };

            var notificationDbSetMock = new Mock<DbSet<Notification>>();
            notificationDbSetMock.Setup(m => m.Where(It.IsAny<Expression<Func<Notification, bool>>>())).Returns(notifications.AsQueryable());

            var notificationStatusDbSetMock = new Mock<DbSet<NotificationStatus>>();
            notificationStatusDbSetMock.Setup(m => m.Where(It.IsAny<Expression<Func<NotificationStatus, bool>>>())).Returns(Enumerable.Empty<NotificationStatus>().AsQueryable());

            dbContextMock.Setup(db => db.Set<Notification>()).Returns(notificationDbSetMock.Object);
            dbContextMock.Setup(db => db.Set<NotificationStatus>()).Returns(notificationStatusDbSetMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            // Act
            var userIds = await notificationRepository.MarkNotificationsAsDeletedByTask(notifications[0].TaskId);

            // Assert
            Assert.NotNull(userIds);
            Assert.True(userIds.Count() > 0);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_InvalidTaskId_ReturnsEmptyList()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var dbContextMock = new Mock<DbContext>();
            var mapperMock = new Mock<IMapper>();
            var notificationRepository = new NotificationRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() }
            };

            var notificationDbSetMock = new Mock<DbSet<Notification>>();
            notificationDbSetMock.Setup(m => m.Where(It.IsAny<Expression<Func<Notification, bool>>>())).Returns(Enumerable.Empty<Notification>().AsQueryable());

            var notificationStatusDbSetMock = new Mock<DbSet<NotificationStatus>>();
            notificationStatusDbSetMock.Setup(m => m.Where(It.IsAny<Expression<Func<NotificationStatus, bool>>>())).Returns(Enumerable.Empty<NotificationStatus>().AsQueryable());

            dbContextMock.Setup(db => db.Set<Notification>()).Returns(notificationDbSetMock.Object);
            dbContextMock.Setup(db => db.Set<NotificationStatus>()).Returns(notificationStatusDbSetMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            // Act
            var userIds = await notificationRepository.MarkNotificationsAsDeletedByTask(Guid.NewGuid());

            // Assert
            Assert.NotNull(userIds);
            Assert.Empty(userIds);
        }
    }
}
