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
        private readonly SecretRepository _secretRepository;

        public SecretRepositoryTests()
        {
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _mapperMock = new Mock<IMapper>();
            _secretRepository = new SecretRepository(_serviceScopeFactoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task RestoreManyByIdAsync_ShouldRestoreSecrets()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0], DeletedDate = DateTime.UtcNow },
                new Secret { Id = ids[1], DeletedDate = DateTime.UtcNow }
            };

            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<Secret>>();
            var transactionMock = new Mock<IDbContextTransaction>();

            dbContextMock.Setup(c => c.Set<Secret>()).Returns(dbSetMock.Object);
            dbContextMock.Setup(c => c.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(s => s.CreateAsyncScope()).Returns(scopeMock.Object);

            // Act
            await _secretRepository.RestoreManyByIdAsync(ids);

            // Assert
            dbSetMock.Verify(s => s.ExecuteUpdateAsync(It.IsAny<Func<EntityPropertyValues, EntityPropertyValues>>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task HardDeleteManyByIdAsync_ShouldDeleteSecrets()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0] },
                new Secret { Id = ids[1] }
            };

            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<Secret>>();
            var transactionMock = new Mock<IDbContextTransaction>();

            dbContextMock.Setup(c => c.Set<Secret>()).Returns(dbSetMock.Object);
            dbContextMock.Setup(c => c.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(s => s.CreateAsyncScope()).Returns(scopeMock.Object);

            // Act
            await _secretRepository.HardDeleteManyByIdAsync(ids);

            // Assert
            dbSetMock.Verify(s => s.ExecuteDeleteAsync(), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateManyByIdAsync_ShouldUpdateSecrets()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var secrets = new List<Secret>
            {
                new Secret { Id = ids[0] },
                new Secret { Id = ids[1] }
            };

            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<Secret>>();
            var transactionMock = new Mock<IDbContextTransaction>();

            dbContextMock.Setup(c => c.Set<Secret>()).Returns(dbSetMock.Object);
            dbContextMock.Setup(c => c.Database.BeginTransactionAsync()).ReturnsAsync(transactionMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);

            _serviceScopeFactoryMock.Setup(s => s.CreateAsyncScope()).Returns(scopeMock.Object);

            // Act
            await _secretRepository.UpdateManyByIdAsync(ids);

            // Assert
            dbSetMock.Verify(s => s.ExecuteUpdateAsync(It.IsAny<Func<EntityPropertyValues, EntityPropertyValues>>()), Times.Once);
            transactionMock.Verify(t => t.CommitAsync(), Times.Once);
        }
    }
}
