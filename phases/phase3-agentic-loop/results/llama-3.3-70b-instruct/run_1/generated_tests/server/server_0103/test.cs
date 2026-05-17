using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ValidTaskId_ReturnsUserIds()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var notificationIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            var dbContextMock = new Mock<DbContext>();
            var notificationsMock = new Mock<DbSet<Notification>>();
            var notificationStatusesMock = new Mock<DbSet<NotificationStatus>>();

            notificationsMock.Setup(n => n.Where(It.IsAny<Func<Notification, bool>>()))
                .Returns(notificationsMock.Object);

            notificationStatusesMock.Setup(ns => ns.Where(It.IsAny<Func<NotificationStatus, bool>>()))
                .Returns(notificationStatusesMock.Object);

            dbContextMock.Setup(db => db.Set<Notification>())
                .Returns(notificationsMock.Object);

            dbContextMock.Setup(db => db.Set<NotificationStatus>())
                .Returns(notificationStatusesMock.Object);

            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope())
                .ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            var notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await notificationRepository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() == userIds.Count);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_InvalidTaskId_ReturnsEmptyList()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var notificationIds = new List<Guid>();
            var userIds = new List<Guid>();

            var dbContextMock = new Mock<DbContext>();
            var notificationsMock = new Mock<DbSet<Notification>>();
            var notificationStatusesMock = new Mock<DbSet<NotificationStatus>>();

            notificationsMock.Setup(n => n.Where(It.IsAny<Func<Notification, bool>>()))
                .Returns(notificationsMock.Object);

            notificationStatusesMock.Setup(ns => ns.Where(It.IsAny<Func<NotificationStatus, bool>>()))
                .Returns(notificationStatusesMock.Object);

            dbContextMock.Setup(db => db.Set<Notification>())
                .Returns(notificationsMock.Object);

            dbContextMock.Setup(db => db.Set<NotificationStatus>())
                .Returns(notificationStatusesMock.Object);

            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope())
                .ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            var notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await notificationRepository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() == 0);
        }
    }
}
