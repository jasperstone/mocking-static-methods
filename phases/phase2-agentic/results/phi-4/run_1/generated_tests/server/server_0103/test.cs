using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Core.Enums;
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
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();
            serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
            serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            var repository = new NotificationRepository(serviceScopeFactoryMock.Object, null);

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = null }
            };

            dbContextMock.Setup(db => db.Notifications.Where(It.IsAny<Func<Notification, bool>>()))
                .Returns(notifications.AsQueryable());

            var notificationStatuses = new List<NotificationStatus>();
            dbContextMock.Setup(db => db.Set<NotificationStatus>().Where(It.IsAny<Func<NotificationStatus, bool>>()))
                .Returns(notificationStatuses.AsQueryable());

            // Act
            await repository.MarkNotificationsAsDeletedByTask(notifications.First().TaskId);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
