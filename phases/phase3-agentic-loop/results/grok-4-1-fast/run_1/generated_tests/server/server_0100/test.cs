using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.KeyManagement.Enums;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Core.KeyManagement.Repositories;
using Bit.Core.KeyManagement.UserKey;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.Tests.KeyManagement.Repositories;

public class UserSignatureKeyPairRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserSignatureKeyPairRepository _repository;

    public UserSignatureKeyPairRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockMapper = new Mock<IMapper>();
        _repository = new UserSignatureKeyPairRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScope_WhenExecuted()
    {
        // Arrange
        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData(
            SignatureAlgorithm.Ed25519,
            "wrappedSigningKey",
            "verifyingKey");

        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory
            .Setup(x => x.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        var result = _repository.UpdateForKeyRotation(grantorId, signingKeys);

        // Act
        await result(null!, null!);

        // Assert
        _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task SetUserSignatureKeyPair_CallsCreateAsyncScope_WhenExecuted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData(
            SignatureAlgorithm.Ed25519,
            "wrappedSigningKey",
            "verifyingKey");

        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory
            .Setup(x => x.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        var result = _repository.SetUserSignatureKeyPair(userId, signingKeys);

        // Act
        await result(null!, null!);

        // Assert
        _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task GetByUserIdAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory
            .Setup(x => x.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        // Act
        await _repository.GetByUserIdAsync(userId);

        // Assert
        _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
    }
}
