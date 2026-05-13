using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.Repositories;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Core.Enums;
using Bit.Core.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Bit.Core.NotificationCenter.Entities;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories.Tests
{
    public class NotificationRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly NotificationRepository _repository;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ShouldMarkNotificationsAsDeleted()
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

            var dbContextMock = new Mock<DatabaseContext>();
            dbContextMock.Setup(db => db.Notifications).ReturnsDbSet(notifications);
            dbContextMock.Setup(db => db.Set<NotificationStatus>()).ReturnsDbSet(notificationStatuses);

            var serviceScopeMock = new Mock<IServiceScope>();
            serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(factory => factory.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            // Act
            var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(notificationStatuses, status => Assert.NotNull(status.DeletedDate));
            dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        }
    }

    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> ReturnsDbSet<T>(this Mock<DbSet<T>> mockSet, List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return mockSet;
        }
    }
}
