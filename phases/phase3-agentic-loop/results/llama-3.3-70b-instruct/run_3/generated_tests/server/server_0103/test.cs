using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Models;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_ValidTaskId_ReturnsUserIds()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<NotificationCenterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new NotificationCenterDbContext(options);
            context.Database.EnsureCreated();

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
            };

            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync();

            var serviceScopeFactory = new ServiceScopeFactory(context);
            var mapper = new Mapper(new MapperConfiguration(mc => mc.AddProfile(new NotificationMapperProfile())));

            var notificationRepository = new NotificationRepository(serviceScopeFactory, mapper);

            // Act
            var userIds = await notificationRepository.MarkNotificationsAsDeletedByTask(notifications.First().TaskId);

            // Assert
            Assert.NotNull(userIds);
            Assert.Single(userIds);
        }

        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_InvalidTaskId_ReturnsEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<NotificationCenterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new NotificationCenterDbContext(options);
            context.Database.EnsureCreated();

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
                new Notification { Id = Guid.NewGuid(), TaskId = Guid.NewGuid(), UserId = Guid.NewGuid() },
            };

            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync();

            var serviceScopeFactory = new ServiceScopeFactory(context);
            var mapper = new Mapper(new MapperConfiguration(mc => mc.AddProfile(new NotificationMapperProfile())));

            var notificationRepository = new NotificationRepository(serviceScopeFactory, mapper);

            // Act
            var userIds = await notificationRepository.MarkNotificationsAsDeletedByTask(Guid.NewGuid());

            // Assert
            Assert.NotNull(userIds);
            Assert.Empty(userIds);
        }
    }
}
