using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task AccessToSecretsAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IAsyncDisposable>();
            var serviceScope = Mock.Of<IServiceScope>(s => s is IAsyncDisposable);
            var serviceScopeAsyncMock = new Mock<IAsyncDisposable>();

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.As<IAsyncDisposable>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .Returns(serviceScope.Object);

            var mapperMock = new Mock<IMapper>();

            var repo = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            // We cannot fully test the internals without a full DbContext mock, but we can verify CreateAsyncScope is called.

            // Act
            var ids = new List<Guid> { Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;

            // We call the method that uses CreateAsyncScope internally
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await repo.AccessToSecretsAsync(ids, userId, accessType));

            // Assert
            serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task GetSecretsCountByOrganizationIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScope = new Mock<IServiceScope>();
            serviceScope.As<IAsyncDisposable>().Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);

            serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .Returns(serviceScope.Object);

            var mapperMock = new Mock<IMapper>();

            var repo = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            // Act
            var organizationId = Guid.NewGuid();

            // We expect NullReferenceException because DbContext is not mocked, but we want to verify CreateAsyncScope call
            await Assert.ThrowsAsync<NullReferenceException>(async () =>
                await repo.GetSecretsCountByOrganizationIdAsync(organizationId, Guid.NewGuid(), AccessClientType.User));

            // Assert
            serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }
}
