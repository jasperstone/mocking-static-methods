using System;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.Tests
{
    public class UserSignatureKeyPairRepositoryTests
    {
        [Fact]
        public async Task SetUserSignatureKeyPair_CreatesScopeAndAddsEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<KeyManagementDbContext>();
            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, null);

            serviceScopeFactoryMock
                .Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider.GetService(typeof(KeyManagementDbContext)))
                .Returns(dbContextMock.Object);

            var userId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "Algorithm",
                WrappedSigningKey = "SigningKey",
                VerifyingKey = "VerifyingKey"
            };

            var entity = new Models.UserSignatureKeyPair
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SignatureAlgorithm = signingKeys.SignatureAlgorithm,
                SigningKey = signingKeys.WrappedSigningKey,
                VerifyingKey = signingKeys.VerifyingKey,
                CreationDate = DateTime.UtcNow,
                RevisionDate = DateTime.UtcNow
            };

            dbContextMock
                .Setup(db => db.UserSignatureKeyPairs.AddAsync(It.IsAny<Models.UserSignatureKeyPair>()))
                .Callback<Models.UserSignatureKeyPair>(e => Assert.Equal(entity, e));

            dbContextMock
                .Setup(db => db.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var updateOperation = repository.SetUserSignatureKeyPair(userId, signingKeys);
            await updateOperation(null, null);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateForKeyRotation_CreatesScopeAndUpdateEntity()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var scopeMock = new Mock<IServiceScope>();
            var dbContextMock = new Mock<KeyManagementDbContext>();
            var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, null);

            serviceScopeFactoryMock
                .Setup(sf => sf.CreateAsyncScope())
                .ReturnsAsync(scopeMock.Object);

            scopeMock
                .Setup(s => s.ServiceProvider.GetService(typeof(KeyManagementDbContext)))
                .Returns(dbContextMock.Object);

            var grantorId = Guid.NewGuid();
            var signingKeys = new SignatureKeyPairData
            {
                SignatureAlgorithm = "Algorithm",
                WrappedSigningKey = "SigningKey",
                VerifyingKey = "VerifyingKey"
            };

            var entity = new Models.UserSignatureKeyPair
            {
                Id = Guid.NewGuid(),
                UserId = grantorId,
                SignatureAlgorithm = "OldAlgorithm",
                SigningKey = "OldSigningKey",
                VerifyingKey = "OldVerifyingKey",
                CreationDate = DateTime.UtcNow.AddDays(-1),
                RevisionDate = DateTime.UtcNow.AddDays(-1)
            };

            dbContextMock
                .Setup(db => db.UserSignatureKeyPairs.FirstOrDefaultAsync(It.IsAny<Func<Models.UserSignatureKeyPair, bool>>()))
                .ReturnsAsync(entity);

            dbContextMock
                .Setup(db => db.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var updateOperation = repository.UpdateForKeyRotation(grantorId, signingKeys);
            await updateOperation(null, null);

            // Assert
            Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
            Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
            Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
            Assert.True(entity.RevisionDate > entity.CreationDate);

            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
            dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
        }
    }
}
