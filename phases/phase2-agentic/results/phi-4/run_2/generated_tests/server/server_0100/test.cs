using System;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class UserSignatureKeyPairRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IDbContext> _dbContextMock;
    private readonly UserSignatureKeyPairRepository _repository;

    public UserSignatureKeyPairRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _dbContextMock = new Mock<IDbContext>();
        var mapperMock = new Mock<IMapper>();

        _repository = new UserSignatureKeyPairRepository(_serviceScopeFactoryMock.Object, mapperMock.Object);
    }

    [Fact]
    public async Task SetUserSignatureKeyPair_CallsCreateAsyncScope()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "Algorithm",
            WrappedSigningKey = "SigningKey",
            VerifyingKey = "VerifyingKey"
        };

        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);

        _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IDbContext))).Returns(_dbContextMock.Object);

        // Act
        var updateOperation = _repository.SetUserSignatureKeyPair(userId, signingKeys);
        await updateOperation(null, null);

        // Assert
        _serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScope()
    {
        // Arrange
        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "Algorithm",
            WrappedSigningKey = "SigningKey",
            VerifyingKey = "VerifyingKey"
        };

        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);

        _serviceScopeFactoryMock.Setup(sf => sf.CreateAsyncScope()).ReturnsAsync(scopeMock.Object);
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IDbContext))).Returns(_dbContextMock.Object);

        // Act
        var updateOperation = _repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateOperation(null, null);

        // Assert
        _serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
    }
}
