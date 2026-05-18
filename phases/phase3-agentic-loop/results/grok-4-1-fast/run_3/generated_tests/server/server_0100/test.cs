using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.Tests;

public class UserSignatureKeyPairRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<global::AutoMapper.IMapper> _mockMapper;
    private readonly Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.UserSignatureKeyPairRepository _repository;

    public UserSignatureKeyPairRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMapper = new Mock<global::AutoMapper.IMapper>();
        _repository = new Bit.Infrastructure.EntityFramework.KeyManagement.Repositories.UserSignatureKeyPairRepository(
            _mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task UpdateForKeyRotation_CreatesAsyncScope()
    {
        // Arrange
        var grantorId = Guid.NewGuid();
        var signingKeys = new Bit.Core.KeyManagement.Models.Data.SignatureKeyPairData
        {
            SignatureAlgorithm = "Ed25519",
            WrappedSigningKey = new byte[] { 1, 2, 3 },
            VerifyingKey = new byte[] { 4, 5, 6 }
        };

        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory.Setup(x => x.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        var updateAction = _repository.UpdateForKeyRotation(grantorId, signingKeys);
        await updateAction(null!, CancellationToken.None);

        // Assert
        _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
    }
}
