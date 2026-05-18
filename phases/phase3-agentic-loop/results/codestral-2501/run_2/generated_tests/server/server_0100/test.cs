using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.UserKey;
using Bit.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Bit.Infrastructure.EntityFramework;
using Bit.Core.KeyManagement.Entities;
using System.Linq;

public class UserSignatureKeyPairRepositoryTests
{
    [Fact]
    public async Task UpdateForKeyRotation_ShouldUpdateEntity_WhenEntityExists()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var mapperMock = new Mock<IMapper>();
        var dbContextMock = new Mock<DatabaseContext>();
        var userSignatureKeyPairMock = new Mock<DbSet<Core.KeyManagement.Entities.UserSignatureKeyPair>>();

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

        var existingEntity = new Core.KeyManagement.Entities.UserSignatureKeyPair
        {
            UserId = grantorId,
            SignatureAlgorithm = "OldAlgorithm",
            SigningKey = "oldSigningKey",
            VerifyingKey = "oldVerifyingKey",
            RevisionDate = DateTime.UtcNow.AddDays(-1)
        };

        var data = new List<Core.KeyManagement.Entities.UserSignatureKeyPair> { existingEntity }.AsQueryable();

        userSignatureKeyPairMock.As<IQueryable<Core.KeyManagement.Entities.UserSignatureKeyPair>>().Setup(m => m.Provider).Returns(data.Provider);
        userSignatureKeyPairMock.As<IQueryable<Core.KeyManagement.Entities.UserSignatureKeyPair>>().Setup(m => m.Expression).Returns(data.Expression);
        userSignatureKeyPairMock.As<IQueryable<Core.KeyManagement.Entities.UserSignatureKeyPair>>().Setup(m => m.ElementType).Returns(data.ElementType);
        userSignatureKeyPairMock.As<IQueryable<Core.KeyManagement.Entities.UserSignatureKeyPair>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        dbContextMock.Setup(x => x.UserSignatureKeyPairs).Returns(userSignatureKeyPairMock.Object);

        // Act
        var updateAction = repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateAction(null, null);

        // Assert
        dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(signingKeys.SignatureAlgorithm, existingEntity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, existingEntity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, existingEntity.VerifyingKey);
        Assert.True(existingEntity.RevisionDate > DateTime.UtcNow.AddDays(-1));
    }
}
