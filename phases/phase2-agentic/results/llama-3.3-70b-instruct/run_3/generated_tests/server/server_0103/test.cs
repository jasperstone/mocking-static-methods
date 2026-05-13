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

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories;

public class NotificationRepositoryTests
{
    [Fact]
    public async Task MarkNotificationsAsDeletedByTask_ValidTaskId_NotificationsMarkedAsDeleted()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BitContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new BitContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var notification1 = new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid() };
        var notification2 = new Notification { Id = Guid.NewGuid(), TaskId = notification1.TaskId };

        dbContext.Notifications.Add(notification1);
        dbContext.Notifications.Add(notification2);

        await dbContext.SaveChangesAsync();

        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceScopeFactoryMock
            .Setup(ssf => ssf.CreateAsyncScope())
            .ReturnsAsync(scopeMock.Object);

        scopeMock
            .SetupGet(ss => ss.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(BitContext)))
            .Returns(dbContext);

        var mapperMock = new Mock<IMapper>();

        var notificationRepository = new NotificationRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        // Act
        var result = await notificationRepository.MarkNotificationsAsDeletedByTask(notification1.TaskId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Any());
    }
}
