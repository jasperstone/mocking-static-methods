using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.NotificationCenter.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.NotificationCenter.Tests.Repositories
{
    public class NotificationRepositoryTests
    {
        [Fact]
        public async Task MarkNotificationsAsDeletedByTask_CallsCreateAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<DbContext>();

            serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
                .Returns(dbContextMock.Object);

            serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(serviceScopeMock.Object);

            var repository = new NotificationRepository(serviceScopeFactoryMock.Object, null);

            var taskId = Guid.NewGuid();

            // Act
            await repository.MarkNotificationsAsDeletedByTask(taskId);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
