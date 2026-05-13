using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Core.Models.Data;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories.Queries;
using Bit.Core.NotificationCenter.Entities;

namespace NotificationRepositoryTests
{
    public class NotificationRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IServiceScope> _serviceScopeMock;
        private readonly Mock<NotificationDbContext> _dbContextMock;
        private readonly IMapper _mapper;

        public NotificationRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<NotificationDbContext>();
            _mapper = new AutoMapper.Mapper(new MapperConfiguration(cfg => { }));

            _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_serviceScopeMock.Object);
            _serviceScopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            _serviceScopeMock.Setup(s => s.GetService(typeof(NotificationDbContext))).Returns(_dbContextMock.Object);
        }

        [Fact]
        public async Task GetByUserIdAsync_CreatesScopeAndCallsRun()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Email;
            var notifications = new List<NotificationStatusDetails>
            {
                new NotificationStatusDetails { Priority = 1, CreationDate = DateTime.UtcNow }
            };

            var queryMock = new Mock<IQueryable<NotificationStatusDetails>>();
            var runMock = new Mock<NotificationStatusDetailsViewQuery>(userId, clientType);
            runMock.Setup(r => r.Run(It.IsAny<NotificationDbContext>())).ReturnsAsync(notifications.AsQueryable());

            // Act
            var repo = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapper);
            var result = await repo.GetByUserIdAsync(userId, clientType);

            // Assert
            Assert.NotNull(result);
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task GetByUserIdAndStatusAsync_CreatesScopeAndReturnsPagedResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var clientType = ClientType.Email;
            var pageOptions = new PageOptions { PageSize = 10, ContinuationToken = "1" };
            var notifications = new List<NotificationStatusDetails>
            {
                new NotificationStatusDetails { Priority = 1, CreationDate = DateTime.UtcNow }
            };

            var queryMock = new Mock<IQueryable<NotificationStatusDetails>>();
            var runMock = new Mock<NotificationStatusDetailsViewQuery>(userId, clientType);
            runMock.Setup(r => r.Run(It.IsAny<NotificationDbContext>())).ReturnsAsync(notifications.AsQueryable());

            // Act
            var repo = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapper);
            var result = await repo.GetByUserIdAndStatusAsync(userId, clientType, null, pageOptions);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PagedResult<NotificationStatusDetails>>(result);
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_CreatesScopeAndUpdatesStatuses()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var notificationId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification { Id = notificationId, UserId = Guid.NewGuid(), TaskId = taskId }
            };
            var statuses = new List<NotificationStatus>
            {
                new NotificationStatus { NotificationId = notificationId, UserId = Guid.NewGuid(), DeletedDate = null }
            };

            _dbContextMock.Setup(db => db.Notifications).ReturnsDbSet(notifications);
            _dbContextMock.Setup(db => db.Set<NotificationStatus>()).ReturnsDbSet(statuses);
            _dbContextMock.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var repo = new NotificationRepository(_serviceScopeFactoryMock.Object, _mapper);
            var result = await repo.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(statuses[0].NotificationId, result);
            _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }
}
