using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.UserKey;
using Bit.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Threading.Tasks;

public class UserSignatureKeyPairRepositoryTests
{
    [Fact]
    public async Task SetUserSignatureKeyPair_ShouldCreateNewUserSignatureKeyPair()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var dbContextMock = new Mock<DatabaseContext>();
        var userSignatureKeyPairsMock = new Mock<DbSet<Models.UserSignatureKeyPair>>();

        dbContextMock.Setup(c => c.UserSignatureKeyPairs).Returns(userSignatureKeyPairsMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);
        serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var userId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "RSA",
            WrappedSigningKey = "signingKey",
            VerifyingKey = "verifyingKey"
        };

        // Act
        var updateEncryptedDataForKeyRotation = repository.SetUserSignatureKeyPair(userId, signingKeys);
        await updateEncryptedDataForKeyRotation(null, null);

        // Assert
        userSignatureKeyPairsMock.Verify(m => m.AddAsync(It.IsAny<Models.UserSignatureKeyPair>(), default), Times.Once);
        dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateForKeyRotation_ShouldUpdateUserSignatureKeyPair()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var dbContextMock = new Mock<DatabaseContext>();
        var userSignatureKeyPairsMock = new Mock<DbSet<Models.UserSignatureKeyPair>>();

        dbContextMock.Setup(c => c.UserSignatureKeyPairs).Returns(userSignatureKeyPairsMock.Object);

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(s => s.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);
        serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "RSA",
            WrappedSigningKey = "signingKey",
            VerifyingKey = "verifyingKey"
        };

        var existingEntity = new Models.UserSignatureKeyPair
        {
            UserId = grantorId,
            SignatureAlgorithm = "OldRSA",
            SigningKey = "oldSigningKey",
            VerifyingKey = "oldVerifyingKey"
        };

        userSignatureKeyPairsMock.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Func<Models.UserSignatureKeyPair, bool>>(), default))
            .ReturnsAsync(existingEntity);

        // Act
        var updateEncryptedDataForKeyRotation = repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateEncryptedDataForKeyRotation(null, null);

        // Assert
        Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
        dbContextMock.Verify(m => m.SaveChangesAsync(default), Times.Once);
    }
}
