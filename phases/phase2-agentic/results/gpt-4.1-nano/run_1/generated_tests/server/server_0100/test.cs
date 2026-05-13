using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using src.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.UserKey;
using Bit.Infrastructure.EntityFramework.Repositories;
using Bit.Core.KeyManagement.Entities;

namespace RepositoryTests
{
    public class UserSignatureKeyPairRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
        private readonly Mock<IServiceScope> _scopeMock;
        private readonly Mock<DbContext> _dbContextMock;
        private readonly Mock<DbSet<UserSignatureKeyPair>> _dbSetMock;
        private readonly UserSignatureKeyPairRepository _repository;

        public UserSignatureKeyPairRepositoryTests()
        {
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _dbContextMock = new Mock<DbContext>();
            _dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();

            _scopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _scopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_scopeMock.Object);

            // Setup GetDatabaseContext to return our mocked DbContext
            _repository = new UserSignatureKeyPairRepository(_scopeFactoryMock.Object, null);
            // Override GetDatabaseContext method if needed, or set up via reflection
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsSignatureKeyPairData_WhenEntityExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var entity = new UserSignatureKeyPair { UserId = userId };
            var data = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 1, 2 },
                VerifyingKey = new byte[] { 3, 4 }
            };

            var queryableData = new[] { entity }.AsQueryable();

            _dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            _dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            _dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            _dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            _dbContextMock.Setup(c => c.Set<UserSignatureKeyPair>()).Returns(_dbSetMock.Object);
            // Setup GetDatabaseContext to return _dbContextMock.Object

            // Act
            var result = await _repository.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("alg", result.SignatureAlgorithm);
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_CreatesAndSavesEntity()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 1 },
                VerifyingKey = new byte[] { 2 }
            };

            var addAsyncCalled = false;
            var dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();
            dbSetMock.Setup(d => d.AddAsync(It.IsAny<UserSignatureKeyPair>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((UserSignatureKeyPair entity, CancellationToken token) =>
                     {
                         addAsyncCalled = true;
                         return (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UserSignatureKeyPair>)null;
                     });

            _dbContextMock.Setup(c => c.Set<UserSignatureKeyPair>()).Returns(dbSetMock.Object);
            // Setup SaveChangesAsync
            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Inject mocked DbContext into repository if needed

            var setKeyAction = _repository.SetUserSignatureKeyPair(userId, signingKeys);

            // Act
            await setKeyAction.Invoke(null, null);

            // Assert
            Assert.True(addAsyncCalled);
            _dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_UpdatesEntity_WhenEntityExists()
        {
            // Arrange
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "alg",
                WrappedSigningKey = new byte[] { 5 },
                VerifyingKey = new byte[] { 6 }
            };

            var entity = new UserSignatureKeyPair { UserId = grantorId };
            var dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();
            var queryableData = new[] { entity }.AsQueryable();

            dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            dbSetMock.As<IQueryable<UserSignatureKeyPair>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            _dbContextMock.Setup(c => c.Set<UserSignatureKeyPair>()).Returns(dbSetMock.Object);
            _dbContextMock.Setup(c => c.UserSignatureKeyPairs).Returns(dbSetMock.Object);
            _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Setup FirstOrDefaultAsync to return our entity
            _dbContextMock.Setup(c => c.UserSignatureKeyPairs.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserSignatureKeyPair, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var updateAction = _repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateAction.Invoke(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            _dbContextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
