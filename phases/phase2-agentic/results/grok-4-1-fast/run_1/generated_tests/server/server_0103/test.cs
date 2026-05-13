using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.NotificationCenter.Entities;
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
        var mockScope = new Mock<IAsyncServiceScope>();
        var mockServiceScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>(new DbContextOptions<DatabaseContext>());

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceScope.Object);
        mockServiceScope.Setup(s => s.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object)
            .Verifiable();

        // Verify the extension method call on IServiceScopeFactory is invoked
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Never); // Pre-verify

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        mockScope.Verify(s => s.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_WithNotificationsAndStatuses_UpdatesCorrectly()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId1 = Guid.NewGuid();
        var notificationId2 = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId1, TaskId = taskId, UserId = userId1 },
            new Notification { Id = notificationId2, TaskId = taskId, UserId = null }
        };

        var existingStatus = new NotificationStatus { NotificationId = notificationId1, UserId = userId1, DeletedDate = null };

        var mockScope = new Mock<IAsyncServiceScope>();
        var mockServiceScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>(new DbContextOptions<DatabaseContext>());

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceScope.Object);
        mockServiceScope.Setup(s => s.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications.Where(It.IsAny<Func<Notification, bool>>()))
            .Returns(notifications.AsQueryable());
        mockDbContext.Setup(c => c.Set<NotificationStatus>().Where(It.IsAny<Func<NotificationStatus, bool>>()))
            .Returns(new List<NotificationStatus> { existingStatus }.AsQueryable());

        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        mockDbContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Contains(userId1, result);
        Assert.DoesNotContain(notificationId2, result.Select(x => x)); // No userId
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_NoNotifications_ReturnsEmpty()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var mockScope = new Mock<IAsyncServiceScope>();
        var mockServiceScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>(new DbContextOptions<DatabaseContext>());

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceScope.Object);
        mockServiceScope.Setup(s => s.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications.Where(It.IsAny<Func<Notification, bool>>()))
            .Returns(Enumerable.Empty<Notification>().AsQueryable());

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_MissingStatus_CreatesNewStatus()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var notification = new Notification { Id = notificationId, TaskId = taskId, UserId = userId };

        var mockScope = new Mock<IAsyncServiceScope>();
        var mockServiceScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DatabaseContext>(new DbContextOptions<DatabaseContext>());

        mockScope.Setup(s => s.ServiceProvider).Returns(mockServiceScope.Object);
        mockServiceScope.Setup(s => s.GetService(typeof(DatabaseContext))).Returns(mockDbContext.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).ReturnsAsync(mockScope.Object);

        mockDbContext.Setup(c => c.Notifications.Where(It.IsAny<Func<Notification, bool>>()))
            .Returns(new[] { notification }.AsQueryable());
        mockDbContext.Setup(c => c.Set<NotificationStatus>().Where(It.IsAny<Func<NotificationStatus, bool>>()))
            .Returns(Enumerable.Empty<NotificationStatus>().AsQueryable());

        mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockDbContext.Object.Notifications as DbSet<NotificationStatus>);
        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        mockDbContext.Verify(c => c.Set<NotificationStatus>().Add(It.Is<NotificationStatus>(ns => 
            ns.NotificationId == notificationId && ns.UserId == userId)), Times.Once);
        Assert.Contains(userId, result);
    }
}
