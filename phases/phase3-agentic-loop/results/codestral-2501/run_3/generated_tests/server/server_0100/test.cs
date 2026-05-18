using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Threading.Tasks;

public class UserSignatureKeyPairRepositoryTests
{
    [Fact]
    public async Task UpdateForKeyRotation_ShouldUpdateEntity_WhenEntityExists()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var dbContextMock = new Mock<DatabaseContext>();
        var userSignatureKeyPairsMock = new Mock<DbSet<UserSignatureKeyPair>>();

        var serviceScopeMock = new Mock<IServiceScope>();
        serviceScopeMock.Setup(x => x.ServiceProvider.GetService(typeof(DatabaseContext))).Returns(dbContextMock.Object);

        serviceScopeFactoryMock.Setup(x => x.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "RSA",
            WrappedSigningKey = "wrappedSigningKey",
            VerifyingKey = "verifyingKey"
        };

        var existingEntity = new UserSignatureKeyPair
        {
            UserId = grantorId,
            SignatureAlgorithm = "OldAlgorithm",
            SigningKey = "oldSigningKey",
            VerifyingKey = "oldVerifyingKey",
            RevisionDate = DateTime.UtcNow.AddDays(-1)
        };

        userSignatureKeyPairsMock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Func<UserSignatureKeyPair, bool>>(), default))
            .ReturnsAsync(existingEntity);

        dbContextMock.Setup(x => x.UserSignatureKeyPairs).Returns(userSignatureKeyPairsMock.Object);

        // Act
        var updateAction = repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateAction(null, null);

        // Assert
        Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
        Assert.True(existingEntity.RevisionDate > DateTime.UtcNow.AddDays(-1));
    }
}
