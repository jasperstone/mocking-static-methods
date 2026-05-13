using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<NotificationCenterContext> _notificationCenterContextMock;
        private readonly Mock<IMapper> _mapperMock;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _notificationCenterContextMock = new Mock<NotificationCenterContext>();
            _mapperMock = new Mock<IMapper>();
        }

        [Fact]
        public async Task GetByUserIdAsync_WithValidUserId_ReturnsNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Web;
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType },
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType }
            };

            _notificationCenterContextMock.Setup(c => c.Notifications).Returns(DbSetMock.Create(notifications));
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).Returns(new AsyncServiceScope(_serviceProviderMock.Object));
            _serviceProviderMock.Setup(p => p.GetService(typeof(NotificationCenterContext))).Returns(_notificationCenterContextMock.Object);

            var notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await notificationRepository.GetByUserIdAsync(userId, clientType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() == 2);
        }

        [Fact]
        public async Task GetByUserIdAndStatusAsync_WithValidUserIdAndStatusFilter_ReturnsNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Web;
            var statusFilter = new NotificationStatusFilter { Read = true, Deleted = false };
            var pageOptions = new PageOptions { PageSize = 10, ContinuationToken = null };
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType, ReadDate = DateTime.UtcNow },
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType, ReadDate = null }
            };

            _notificationCenterContextMock.Setup(c => c.Notifications).Returns(DbSetMock.Create(notifications));
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).Returns(new AsyncServiceScope(_serviceProviderMock.Object));
            _serviceProviderMock.Setup(p => p.GetService(typeof(NotificationCenterContext))).Returns(_notificationCenterContextMock.Object);

            var notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await notificationRepository.GetByUserIdAndStatusAsync(userId, clientType, statusFilter, pageOptions);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Data.Count() == 1);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_WithValidTaskId_ReturnsUserIds()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() }
            };

            _notificationCenterContextMock.Setup(c => c.Notifications).Returns(DbSetMock.Create(notifications));
            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).Returns(new AsyncServiceScope(_serviceProviderMock.Object));
            _serviceProviderMock.Setup(p => p.GetService(typeof(NotificationCenterContext))).Returns(_notificationCenterContextMock.Object);

            var notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);

            // Act
            var result = await notificationRepository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count() == 2);
        }
    }

    public static class DbSetMock
    {
        public static DbSet<T> Create<T>(IEnumerable<T> source) where T : class
        {
            var queryable = source.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet.Object;
        }
    }

    public class AsyncServiceScope : IServiceScope
    {
        private readonly IServiceProvider _serviceProvider;

        public AsyncServiceScope(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Dispose()
        {
        }

        public IServiceProvider ServiceProvider => _serviceProvider;
    }
}
