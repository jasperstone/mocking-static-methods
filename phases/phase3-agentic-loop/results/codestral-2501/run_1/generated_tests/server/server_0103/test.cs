using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Core.Enums;
using Bit.Infrastructure.EntityFramework.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ShouldMarkNotificationsAsDeleted()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DatabaseContext>();
            var mapperMock = new Mock<IMapper>();

            serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);

            var notificationRepository = new NotificationRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var taskId = Guid.NewGuid();
            var notifications = new List<Core.NotificationCenter.Entities.Notification>
            {
                new Core.NotificationCenter.Entities.Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() },
                new Core.NotificationCenter.Entities.Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() }
            };

            var notificationStatuses = new List<NotificationStatus>
            {
                new NotificationStatus { NotificationId = notifications[0].Id, DeletedDate = null },
                new NotificationStatus { NotificationId = notifications[1].Id, DeletedDate = null }
            };

            dbContextMock.Setup(x => x.Notifications).ReturnsDbSet(notifications);
            dbContextMock.Setup(x => x.Set<NotificationStatus>()).ReturnsDbSet(notificationStatuses);

            // Act
            var result = await notificationRepository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(notificationStatuses, status => Assert.NotNull(status.DeletedDate));
        }
    }

    public static class MockDbSetExtensions
    {
        public static DbSet<T> ReturnsDbSet<T>(this Mock<DatabaseContext> dbContextMock, List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSetMock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => data.Add(s));
            return dbSetMock.Object;
        }
    }
}
