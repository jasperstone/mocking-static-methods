using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.Repositories;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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
        public async Task MarkNotificationsAsDeletedByTask_ShouldCreateAsyncScope()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var scopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DatabaseContext>();
            var notificationMock = new Mock<Core.NotificationCenter.Entities.Notification>();
            var notificationStatusMock = new Mock<NotificationStatus>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
            scopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);

            var notifications = new List<Core.NotificationCenter.Entities.Notification> { notificationMock.Object };
            var statuses = new List<NotificationStatus> { notificationStatusMock.Object };

            dbContextMock.Setup(x => x.Notifications).ReturnsDbSet(notifications);
            dbContextMock.Setup(x => x.Set<NotificationStatus>()).ReturnsDbSet(statuses);

            // Act
            await _repository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
        }
    }

    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> ReturnsDbSet<T>(this Mock<DatabaseContext> dbContextMock, List<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.AsQueryable().Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.AsQueryable().Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.AsQueryable().ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}
