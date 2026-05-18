using System;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class UserSignatureKeyPairRepositoryTests
{
    [Fact]
    public async Task SetUserSignatureKeyPair_CreatesScopeAndAddsEntity()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<DbContext>();
        var mapperMock = new Mock<IMapper>();

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateAsyncScope())
            .ReturnsAsync(scopeMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<DbContext>())
            .Returns(dbContextMock.Object);

        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var userId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "Algorithm",
            WrappedSigningKey = "SigningKey",
            VerifyingKey = "VerifyingKey"
        };

        // Act
        var updateOperation = repository.SetUserSignatureKeyPair(userId, signingKeys);
        await updateOperation(null, null);

        // Assert
        dbContextMock.Verify(db => db.UserSignatureKeyPairs.AddAsync(It.IsAny<Models.UserSignatureKeyPair>()), Times.Once);
        dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateForKeyRotation_CreatesScopeAndUpdateEntity()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<DbContext>();
        var mapperMock = new Mock<IMapper>();

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateAsyncScope())
            .ReturnsAsync(scopeMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<DbContext>())
            .Returns(dbContextMock.Object);

        var entity = new Models.UserSignatureKeyPair
        {
            UserId = Guid.NewGuid(),
            SignatureAlgorithm = "Algorithm",
            SigningKey = "SigningKey",
            VerifyingKey = "VerifyingKey"
        };

        dbContextMock
            .Setup(db => db.UserSignatureKeyPairs.FirstOrDefaultAsync(It.IsAny<Func<Models.UserSignatureKeyPair, bool>>()))
            .ReturnsAsync(entity);

        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var grantorId = entity.UserId;
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "NewAlgorithm",
            WrappedSigningKey = "NewSigningKey",
            VerifyingKey = "NewVerifyingKey"
        };

        // Act
        var updateOperation = repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateOperation(null, null);

        // Assert
        Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
        dbContextMock.Verify(db => db.SaveChangesAsync(), Times.Once);
    }
}
