using System;
using System.Threading;
using System.Threading.Tasks;
using Bit.Core.KeyManagement.Models.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language.Flow;
using Xunit;
using AutoMapper;

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
        _repository = new UserSignatureKeyPairRepository(_mockServiceScopeFactory.Object, _mockMapper.Object, null!);
    }

    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScope()
    {
        // Arrange
        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "Ed25519",
            WrappedSigningKey = Array.Empty<byte>(),
            VerifyingKey = Array.Empty<byte>()
        };

        var mockScope = new Mock<IAsyncDisposable>();
        _mockServiceScopeFactory.Setup(x => x.CreateAsyncScope())
            .ReturnsAsync(mockScope.Object);

        var updateFunc = _repository.UpdateForKeyRotation(grantorId, signingKeys);

        // Act
        await updateFunc(null!, default(CancellationToken));

        // Assert
        _mockServiceScopeFactory.Verify(x => x.CreateAsyncScope(), Times.Once);
        mockScope.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
