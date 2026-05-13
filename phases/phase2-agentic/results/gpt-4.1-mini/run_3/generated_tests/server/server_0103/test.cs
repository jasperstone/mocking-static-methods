using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_ShouldMarkDeletedAndReturnUserIds()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var notificationId1 = Guid.NewGuid();
        var notificationId2 = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var notifications = new List<Notification>
        {
            new Notification { Id = notificationId1, TaskId = taskId, UserId = userId1 },
            new Notification { Id = notificationId2, TaskId = taskId, UserId = userId2 }
        }.AsQueryable();

        var statuses = new List<NotificationStatus>
        {
            new NotificationStatus { NotificationId = notificationId1, DeletedDate = null }
        }.AsQueryable();

        // Mock DbSet for Notifications
        var mockNotificationsDbSet = new Mock<DbSet<Notification>>();
        mockNotificationsDbSet.As<IQueryable<Notification>>().Setup(m => m.Provider).Returns(notifications.Provider);
        mockNotificationsDbSet.As<IQueryable<Notification>>().Setup(m => m.Expression).Returns(notifications.Expression);
        mockNotificationsDbSet.As<IQueryable<Notification>>().Setup(m => m.ElementType).Returns(notifications.ElementType);
        mockNotificationsDbSet.As<IQueryable<Notification>>().Setup(m => m.GetEnumerator()).Returns(notifications.GetEnumerator());

        // Mock DbSet for NotificationStatus
        var mockStatusesDbSet = new Mock<DbSet<NotificationStatus>>();
        mockStatusesDbSet.As<IQueryable<NotificationStatus>>().Setup(m => m.Provider).Returns(statuses.Provider);
        mockStatusesDbSet.As<IQueryable<NotificationStatus>>().Setup(m => m.Expression).Returns(statuses.Expression);
        mockStatusesDbSet.As<IQueryable<NotificationStatus>>().Setup(m => m.ElementType).Returns(statuses.ElementType);
        mockStatusesDbSet.As<IQueryable<NotificationStatus>>().Setup(m => m.GetEnumerator()).Returns(statuses.GetEnumerator());

        // Setup Add method to track added NotificationStatus
        var addedStatuses = new List<NotificationStatus>();
        mockStatusesDbSet.Setup(m => m.Add(It.IsAny<NotificationStatus>())).Callback<NotificationStatus>(addedStatuses.Add);

        // Mock DbContext
        var mockDbContext = new Mock<NotificationDbContext>();
        mockDbContext.Setup(c => c.Notifications).Returns(mockNotificationsDbSet.Object);
        mockDbContext.Setup(c => c.Set<NotificationStatus>()).Returns(mockStatusesDbSet.Object);
        mockDbContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

        // Mock IServiceScope
        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(NotificationDbContext))).Returns(mockDbContext.Object);

        // Mock IServiceScopeFactory
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(f => f.CreateAsyncScope()).Returns(new AsyncServiceScope(mockScope.Object));

        // Create repository instance with mocked scope factory and mapper
        var repository = new NotificationRepository(mockScopeFactory.Object, null!);

        // Override GetDatabaseContext to return our mockDbContext
        repository.GetType().GetMethod("GetDatabaseContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(repository, new object[] { mockScope.Object });

        // Act
        var result = await repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        Assert.Contains(userId1, result);
        Assert.Contains(userId2, result);

        // The existing status should have DeletedDate set
        var existingStatus = statuses.First();
        Assert.NotNull(existingStatus.DeletedDate);

        // A new status should be added for notificationId2
        Assert.Single(addedStatuses);
        Assert.Equal(notificationId2, addedStatuses[0].NotificationId);
        Assert.Equal(userId2, addedStatuses[0].UserId);
        Assert.NotNull(addedStatuses[0].DeletedDate);

        mockDbContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
    }

    // Helper class to mock IAsyncDisposable IServiceScope returned by CreateAsyncScope
    private class AsyncServiceScope : IAsyncDisposable
    {
        public IServiceScope ServiceScope { get; }

        public AsyncServiceScope(IServiceScope serviceScope)
        {
            ServiceScope = serviceScope;
        }

        public ValueTask DisposeAsync()
        {
            ServiceScope.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
