using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests.Repositories
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task UpdateForKeyRotation_CreatesNewScopeAndUpdatesEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var dbContextMock = new Mock<DbContext>();
            var userSignatureKeyPairRepository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "TestAlgorithm",
                WrappedSigningKey = "TestWrappedSigningKey",
                VerifyingKey = "TestVerifyingKey"
            };

            var entity = new UserSignatureKeyPair
            {
                Id = Guid.NewGuid(),
                UserId = grantorId,
                SignatureAlgorithm = "OldAlgorithm",
                SigningKey = "OldWrappedSigningKey",
                VerifyingKey = "OldVerifyingKey",
                CreationDate = DateTime.UtcNow,
                RevisionDate = DateTime.UtcNow
            };

            dbContextMock.Setup(db => db.UserSignatureKeyPairs).Returns(DbSetMock.Create(new[] { entity }));

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            // Act
            var updateEncryptedDataForKeyRotation = userSignatureKeyPairRepository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

            // Assert
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
            Assert.NotEqual(entity.CreationDate, entity.RevisionDate);
        }

        [Fact]
        public async Task SetUserSignatureKeyPair_CreatesNewScopeAndAddsEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();
            var dbContextMock = new Mock<DbContext>();
            var userSignatureKeyPairRepository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "TestAlgorithm",
                WrappedSigningKey = "TestWrappedSigningKey",
                VerifyingKey = "TestVerifyingKey"
            };

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(new IServiceScope(new ServiceScope(dbContextMock.Object)));

            // Act
            var updateEncryptedDataForKeyRotation = userSignatureKeyPairRepository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

            // Assert
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
        }
    }
}
