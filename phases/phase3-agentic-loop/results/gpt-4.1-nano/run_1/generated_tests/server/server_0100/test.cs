using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Core.KeyManagement.UserKey;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Linq;

namespace Bit.Tests.Infrastructure.EntityFramework.KeyManagement.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<Models.UserSignatureKeyPair>> _dbSetMock;
        private readonly UserSignatureKeyPairRepository _repository;

        public UserSignatureKeyPairRepositoryTests()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<Models.UserSignatureKeyPair>>();

            _scopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _scopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_scopeMock.Object);

            // Setup DbContext to return DbSet
            _dbContextMock.Setup(c => c.Set<Models.UserSignatureKeyPair>()).Returns(_dbSetMock.Object);

            // Instantiate repository with mocked dependencies
            _repository = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object, null);
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_CreatesAndSavesEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 1, 2 },
                VerifyingKey = new byte[] { 3, 4 }
            };

            // Setup AddAsync to simulate adding entity
            var addedEntity = (Models.UserSignatureKeyPair)null;
            _dbSetMock.Setup(d => d.AddAsync(It.IsAny<Models.UserSignatureKeyPair>(), default))
                .ReturnsAsync((Models.UserSignatureKeyPair entity, CancellationToken token) =>
                {
                    addedEntity = entity;
                    return new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Models.UserSignatureKeyPair>(entity);
                });

            // Act
            var updateFunc = _repository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            _scopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            _dbSetMock.Verify(d => d.AddAsync(It.IsAny<Models.UserSignatureKeyPair>(), default), Times.Once);
            _dbContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
            Assert.NotNull(addedEntity);
            Assert.Equal(userId, addedEntity.UserId);
            Assert.Equal("alg", addedEntity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, addedEntity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, addedEntity.VerifyingKey);
        }

        [Fact]
        public async Task UpdateForKeyRotation_UpdatesExistingEntity()
        {
            // Arrange
            var grantorId = Guid.NewGuid();
            var existingEntity = new Models.UserSignatureKeyPair
            {
                UserId = grantorId,
                SignatureAlgorithm = "old",
                SigningKey = new byte[] { 0 },
                VerifyingKey = new byte[] { 0 },
                RevisionDate = DateTime.UtcNow.AddDays(-1)
            };

            _dbSetMock.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Models.UserSignatureKeyPair, bool>>>(), default))
                .ReturnsAsync(existingEntity);

            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "new",
                WrappedSigningKey = new byte[] { 5, 6 },
                VerifyingKey = new byte[] { 7, 8 }
            };

            // Act
            var updateFunc = _repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            _scopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
            _dbSetMock.Verify(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Models.UserSignatureKeyPair, bool>>>(), default), Times.Once);
            _dbContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);
            Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
        }
    }
}
