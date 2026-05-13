using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SecretRepository _repository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _repository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task HardDeleteManyByIdAsync_ShouldDeleteSecrets()
        {
            // Arrange
            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<BitDbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(BitDbContext))).Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            dbContextMock.Setup(x => x.Secret.Where(It.IsAny<Func<Secret, bool>>()).ExecuteDeleteAsync()).Returns(Task.CompletedTask);

            // Act
            await _repository.HardDeleteManyByIdAsync(secretIds);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(x => x.Database.BeginTransactionAsync(), Times.Once);
            dbContextMock.Verify(x => x.Secret.Where(It.IsAny<Func<Secret, bool>>()).ExecuteDeleteAsync(), Times.Once);
            transactionMock.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task RestoreManyByIdAsync_ShouldRestoreSecrets()
        {
            // Arrange
            var secretIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var serviceScopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<BitDbContext>();
            var transactionMock = new Mock<IDbContextTransaction>();

            _serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);
            serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(BitDbContext))).Returns(dbContextMock.Object);
            dbContextMock.Setup(x => x.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);
            dbContextMock.Setup(x => x.Secret.Where(It.IsAny<Func<Secret, bool>>()).ExecuteUpdateAsync(It.IsAny<Action<EntityEntry<Secret>>>())).Returns(Task.CompletedTask);

            // Act
            await _repository.RestoreManyByIdAsync(secretIds);

            // Assert
            _serviceScopeFactoryMock.Verify(x => x.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(x => x.Database.BeginTransactionAsync(), Times.Once);
            dbContextMock.Verify(x => x.Secret.Where(It.IsAny<Func<Secret, bool>>()).ExecuteUpdateAsync(It.IsAny<Action<EntityEntry<Secret>>>()), Times.Once);
            transactionMock.Verify(x => x.CommitAsync(), Times.Once);
        }
    }
}
