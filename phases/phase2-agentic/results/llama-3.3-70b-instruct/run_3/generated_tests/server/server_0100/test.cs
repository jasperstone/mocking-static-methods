using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.Tests
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task UpdateForKeyRotation_CreatesNewScope()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData();

            // Act
            var updateForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateForKeyRotation(null, null);

            // Assert
            serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_UpdatesExistingEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData();
            var entity = new UserSignatureKeyPair { UserId = grantorId };

            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();
            dbSetMock.Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserSignatureKeyPair, bool>>>())).ReturnsAsync(entity);
            dbContextMock.Setup(d => d.UserSignatureKeyPairs).Returns(dbSetMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);

            // Act
            var updateForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateForKeyRotation(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
            Assert.NotNull(entity.RevisionDate);
        }

        [Fact]
        public async Task UpdateForKeyRotation_DoesNotUpdateNonExistingEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);
            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData();

            var dbContextMock = new Mock<DbContext>();
            var dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();
            dbSetMock.Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserSignatureKeyPair, bool>>>())).ReturnsAsync((UserSignatureKeyPair)null);
            dbContextMock.Setup(d => d.UserSignatureKeyPairs).Returns(dbSetMock.Object);

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DbContext))).Returns(dbContextMock.Object);
            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);

            // Act
            var updateForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateForKeyRotation(null, null);

            // Assert
            dbContextMock.Verify(d => d.SaveChangesAsync(), Times.Never);
        }
    }
}
