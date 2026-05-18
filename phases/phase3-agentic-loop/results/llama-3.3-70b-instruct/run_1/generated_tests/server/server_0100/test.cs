using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Core.KeyManagement.UserKey;
using Microsoft.EntityFrameworkCore;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests.Repositories
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
            var updateEncryptedDataForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

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
            dbSetMock.Setup(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserSignatureKeyPair, bool>>>(), default, default, default)).ReturnsAsync(entity);
            dbContextMock.Setup(d => d.Set<UserSignatureKeyPair>()).Returns(dbSetMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).Returns(new ServiceScope(dbContextMock.Object));

            // Act
            var updateEncryptedDataForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
            Assert.NotEqual(default, entity.RevisionDate);
        }
    }
}
