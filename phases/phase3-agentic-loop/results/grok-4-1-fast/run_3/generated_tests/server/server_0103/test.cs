using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Set<Notification>()).ReturnsDbSet(new List<Notification>());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
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
        var mockDbContext = new Mock<DbContext>();
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

        mockDbContext.Setup(c => c.Notifications).Returns(notifications.AsQueryable().AsQueryableMockDbSet());
        mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(statuses.AsQueryable().AsQueryableMockDbSet());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);
        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Single(result);
        Assert.Equal(userId, Assert.Single(result));
        Assert.NotNull(statuses[0].DeletedDate);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_NoNotifications_ReturnsEmpty()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DbContext>();
        var emptyNotifications = new List<Notification>();

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications).Returns(emptyNotifications.AsQueryable().AsQueryableMockDbSet());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
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
        var mockDbContext = new Mock<DbContext>();
        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId, TaskId = taskId, UserId = null }
        };

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications).Returns(notifications.AsQueryable().AsQueryableMockDbSet());
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Empty(result);
    }
}

// Extension method for easy DbSet mocking
public static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> AsQueryableMockDbSet<T>(this IQueryable<T> source) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => source.GetEnumerator());
        return mockSet;
    }
}
