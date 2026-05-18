using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Core.Models.Data;
using Bit.Core.NotificationCenter.Models.Data;
using Bit.Core.NotificationCenter.Models.Filter;
using Bit.Core.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
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

        _mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);

        // Mock DbSet behaviors to avoid actual queries
        var mockNotificationsDbSet = new Mock<DbSet<Bit.Infrastructure.EntityFramework.NotificationCenter.Models.Notification>>();
        mockDbContext.Setup(c => c.Set<Bit.Infrastructure.EntityFramework.NotificationCenter.Models.Notification>())
            .Returns(mockNotificationsDbSet.Object);
        mockNotificationsDbSet.Setup(db => db.Where(It.IsAny<Expression<Func<Bit.Infrastructure.EntityFramework.NotificationCenter.Models.Notification, bool>>>()))
            .Returns(mockNotificationsDbSet.Object);
        mockNotificationsDbSet.Setup(db => db.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Bit.Infrastructure.EntityFramework.NotificationCenter.Models.Notification>());

        var mockStatusDbSet = new Mock<DbSet<NotificationStatus>>();
        mockDbContext.Setup(c => c.Set<NotificationStatus>())
            .Returns(mockStatusDbSet.Object);
        mockStatusDbSet.Setup(db => db.Where(It.IsAny<Expression<Func<NotificationStatus, bool>>>()))
            .Returns(mockStatusDbSet.Object);
        mockStatusDbSet.Setup(db => db.ToListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationStatus>());

        mockDbContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.MarkNotificationsAsDeletedByTask(taskId);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var clientType = ClientType.Web;
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.GetByUserIdAsync(userId, clientType);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAndStatusAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var clientType = ClientType.Web;
        var statusFilter = new NotificationStatusFilter();
        var pageOptions = new PageOptions { PageSize = 10 };
        var mockScope = new Mock<IServiceScope>();
        var mockDbContext = new Mock<DbContext>();

        _mockServiceScopeFactory
            .Setup(f => f.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        mockScope.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(mockDbContext.Object);

        // Act
        var result = await _repository.GetByUserIdAndStatusAsync(userId, clientType, statusFilter, pageOptions);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }
}
