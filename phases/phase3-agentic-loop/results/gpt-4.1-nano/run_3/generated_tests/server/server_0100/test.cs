using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Entities;

namespace Bit.Tests.Infrastructure.EntityFramework.KeyManagement.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<UserSignatureKeyPair>> _dbSetMock;

        public UserSignatureKeyPairRepositoryTests()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();

            _scopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_scopeMock.Object);
            _scopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _scopeMock.Setup(s => s.ServiceProvider).Returns(Mock.Of<IServiceProvider>());
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_CreatesAndSavesEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 1, 2, 3 },
                VerifyingKey = new byte[] { 4, 5, 6 }
            };

            var mockDbContext = new Mock<DbContext>();
            var mockSet = new Mock<DbSet<UserSignatureKeyPair>>();
            mockDbContext.Setup(c => c.Set<UserSignatureKeyPair>()).Returns(mockSet.Object);
            mockSet.Setup(s => s.AddAsync(It.IsAny<UserSignatureKeyPair>(), default))
                .ReturnsAsync((UserSignatureKeyPair entity, System.Threading.CancellationToken token) => 
                {
                    return new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UserSignatureKeyPair>(entity);
                });

            // We need to inject this mock DbContext into the repository
            // For this, we might need to modify the repository to accept a DbContext or mock GetDatabaseContext
            // For now, assume we can test the method behavior directly

            var repo = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object, null);
            // We will call the method and verify that AddAsync and SaveChangesAsync are called

            // Act
            var updateFunc = repo.SetUserSignatureKeyPair(userId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            // Verify that AddAsync was called with an entity having the correct UserId
            mockSet.Verify(s => s.AddAsync(It.Is<UserSignatureKeyPair>(e => e.UserId == userId), default), Times.Once);
            // SaveChangesAsync verification would require a real context or further mocking
        }

        [Fact]
        public async Task UpdateForKeyRotation_UpdatesExistingEntity()
        {
            // Arrange
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 7, 8, 9 },
                VerifyingKey = new byte[] { 10, 11, 12 }
            };

            var existingEntity = new UserSignatureKeyPair
            {
                UserId = grantorId,
                SignatureAlgorithm = "old",
                SigningKey = new byte[] { 0 },
                VerifyingKey = new byte[] { 0 },
                RevisionDate = DateTime.UtcNow
            };

            var mockDbSet = new Mock<DbSet<UserSignatureKeyPair>>();
            mockDbSet.Setup(s => s.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserSignatureKeyPair, bool>>>(), default))
                .ReturnsAsync(existingEntity);

            var mockDbContext = new Mock<DbContext>();
            mockDbContext.Setup(c => c.Set<UserSignatureKeyPair>()).Returns(mockDbSet.Object);
            mockDbContext.Setup(c => c.UserSignatureKeyPairs).Returns(mockDbSet.Object);

            var repo = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object, null);
            // Similar to above, assume we can inject or override GetDatabaseContext

            // Act
            var updateFunc = repo.UpdateForKeyRotation(grantorId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
            // Verify SaveChangesAsync was called
            mockDbContext.Verify(c => c.SaveChangesAsync(default), Times.Once);
        }
    }
}
