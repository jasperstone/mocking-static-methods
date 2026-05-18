using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Core.KeyManagement.UserKey;
using Bit.Core.Utilities;
using Bit.Infrastructure.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.Tests
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

            // Act
            var updateEncryptedDataForKeyRotation = userSignatureKeyPairRepository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

            // Assert
            serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
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

            // Act
            var updateEncryptedDataForKeyRotation = userSignatureKeyPairRepository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateEncryptedDataForKeyRotation(null, null);

            // Assert
            serviceScopeFactoryMock.Verify(ssf => ssf.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
        }
    }
}
