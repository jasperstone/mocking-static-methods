using System;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class UserSignatureKeyPairRepositoryTests
{
    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScope()
    {
        // Arrange
        var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var dbContextMock = new Mock<DbContext>();
        var repository = new UserSignatureKeyPairRepository(serviceScopeFactoryMock.Object, null);

        serviceScopeFactoryMock
            .Setup(sf => sf.CreateAsyncScope())
            .ReturnsAsync(scopeMock.Object);

        scopeMock
            .Setup(s => s.ServiceProvider.GetService(typeof(DbContext)))
            .Returns(dbContextMock.Object);

        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "Algorithm",
            WrappedSigningKey = "SigningKey",
            VerifyingKey = "VerifyingKey"
        };

        // Act
        var updateOperation = repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateOperation(null, null);

        // Assert
        serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
    }
}
