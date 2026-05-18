using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.Enums;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories.Tests;

public class NotificationRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IMapper> _mockMapper;
    private readonly NotificationRepository _repository;

    public NotificationRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMapper = new Mock<IMapper>();
        _repository = new NotificationRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_CallsCreateAsyncScope()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications).ReturnsDbSet(new List<Notification>());
        mockDbContext.Setup(c => c.Set<NotificationStatus>()).ReturnsDbSet(new List<NotificationStatus>());

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_WithNotificationsAndStatuses_UpdatesDeletedDate()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>();
        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId, TaskId = taskId, UserId = userId }
        };
        var statuses = new List<NotificationStatus>
        {
            new NotificationStatus { NotificationId = notificationId, UserId = userId, DeletedDate = null }
        };

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        var mockNotificationsDbSet = notifications.AsQueryable().BuildMockDbSet();
        mockDbContext.Setup(c => c.Notifications).Returns(mockNotificationsDbSet);

        var mockStatusesDbSet = statuses.AsQueryable().BuildMockDbSet();
        mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockStatusesDbSet);

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext)))
            .Returns(mockDbContext.Object);

        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Single(result);
        Assert.Equal(userId, Assert.Single(result));
        Assert.NotNull(statuses[0].DeletedDate);
        mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_NoNotifications_ReturnsEmpty()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>();
        var emptyNotifications = new List<Notification>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications).Returns(emptyNotifications.AsQueryable().BuildMockDbSet());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_NotificationWithoutUserId_SkipsStatusCreation()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>();
        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId, TaskId = taskId, UserId = null }
        };

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications).Returns(notifications.AsQueryable().BuildMockDbSet());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Empty(result);
    }
}

public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> data) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        mockSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(data.ToList());
        return mockSet;
    }
}
