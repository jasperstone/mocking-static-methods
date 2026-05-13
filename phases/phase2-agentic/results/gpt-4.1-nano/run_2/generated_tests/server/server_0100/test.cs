using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Entities;
using Bit.Core.Utilities;

namespace Bit.Tests.Infrastructure.EntityFramework.KeyManagement.Repositories
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

            _scopeFactoryMock.Setup(f => f.CreateAsyncScope())
                .ReturnsAsync(_scopeMock.Object);
            _scopeMock.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);
            _scopeMock.Setup(s => s.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());

            // Setup GetDatabaseContext to return mocked DbContext
            // Since GetDatabaseContext is a protected method, we need to create a derived class for testing
            var repo = new TestUserSignatureKeyPairRepository(_scopeFactoryMock.Object, null);
            _repository = repo;
        }

        private class TestUserSignatureKeyPairRepository : UserSignatureKeyPairRepository
        {
            public TestUserSignatureKeyPairRepository(IServiceScopeFactory scopeFactory, IMapper mapper)
                : base(scopeFactory, mapper)
            {
            }

            public new IServiceScopeFactory ServiceScopeFactory => base.ServiceScopeFactory;

            public new CoreDbContext GetDatabaseContext(IServiceScope scope)
            {
                // Return a mocked DbContext
                return (CoreDbContext)_dbContextMock.Object;
            }
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

            var entity = new UserSignatureKeyPair
            {
                Id = CoreHelpers.GenerateComb(),
                UserId = userId,
                SignatureAlgorithm = signingKeys.SignatureAlgorithm,
                SigningKey = signingKeys.WrappedSigningKey,
                VerifyingKey = signingKeys.VerifyingKey,
                CreationDate = DateTime.UtcNow,
                RevisionDate = DateTime.UtcNow,
            };

            _dbContextMock.Setup(db => db.Set<UserSignatureKeyPair>()).Returns(_dbSetMock.Object);
            _dbContextMock.Setup(db => db.UserSignatureKeyPairs).Returns(_dbSetMock.Object);
            _dbSetMock.Setup(m => m.AddAsync(It.IsAny<UserSignatureKeyPair>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserSignatureKeyPair u, CancellationToken c) => { return u; });
            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var updateFunc = _repository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            _dbSetMock.Verify(m => m.AddAsync(It.Is<UserSignatureKeyPair>(e => e.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

            _dbContextMock.Setup(db => db.UserSignatureKeyPairs).Returns(_dbSetMock.Object);
            _dbSetMock.Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserSignatureKeyPair, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingEntity);
            _dbContextMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var updateFunc = _repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateFunc.Invoke(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
