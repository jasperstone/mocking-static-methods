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
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Notification>> _notificationsDbSetMock;
        private readonly Mock<DbSet<NotificationStatus>> _notificationStatusesDbSetMock;
        private readonly NotificationRepository _notificationRepository;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<DbContext>();
            _notificationsDbSetMock = new Mock<DbSet<Notification>>();
            _notificationStatusesDbSetMock = new Mock<DbSet<NotificationStatus>>();

            _serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(_serviceProviderMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(_dbContextMock.Object);
            _dbContextMock.Setup(db => db.Set<Notification>()).Returns(_notificationsDbSetMock.Object);
            _dbContextMock.Setup(db => db.Set<NotificationStatus>()).Returns(_notificationStatusesDbSetMock.Object);

            _notificationRepository = new NotificationRepository(_serviceScopeFactoryMock.Object, new MapperConfiguration(mc => mc.AddProfile<MappingProfile>()).CreateMapper());
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Web;
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType },
                new Notification { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType },
            };

            _notificationsDbSetMock.Setup(n => n.ToListAsync()).ReturnsAsync(notifications);

            // Act
            var result = await _notificationRepository.GetByUserIdAsync(userId, clientType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByUserIdAndStatusAsync_ReturnsNotifications()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Web;
            var statusFilter = new NotificationStatusFilter { Read = true, Deleted = false };
            var pageOptions = new PageOptions { PageSize = 10, ContinuationToken = "1" };
            var notifications = new List<NotificationStatusDetails>
            {
                new NotificationStatusDetails { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType, ReadDate = DateTime.UtcNow },
                new NotificationStatusDetails { Id = Guid.NewGuid(), UserId = userId, ClientType = clientType, ReadDate = DateTime.UtcNow },
            };

            _notificationsDbSetMock.Setup(n => n.ToListAsync()).ReturnsAsync(notifications);

            // Act
            var result = await _notificationRepository.GetByUserIdAndStatusAsync(userId, clientType, statusFilter, pageOptions);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ReturnsUserIds()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = taskId, UserId = Guid.NewGuid() },
            };

            _notificationsDbSetMock.Setup(n => n.Where(It.IsAny<Func<Notification, bool>>())).Returns(notifications);
            _notificationStatusesDbSetMock.Setup(ns => ns.Where(It.IsAny<Func<NotificationStatus, bool>>())).Returns(new List<NotificationStatus>());

            // Act
            var result = await _notificationRepository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}
