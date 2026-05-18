using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.KeyManagement.Models.Data;
using Bit.Infrastructure.EntityFramework.KeyManagement.Repositories;
using Bit.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Infrastructure.EntityFramework.KeyManagement.Tests;

public class UserSignatureKeyPairRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<DatabaseContext> _dbContextMock;
    private readonly Mock<DbSet<UserSignatureKeyPair>> _dbSetMock;
    private readonly IMapper _mapper;

    public UserSignatureKeyPairRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _dbContextMock = new Mock<DatabaseContext>();
        _dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();

        // Setup IServiceScopeFactory to return IServiceScope
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScope to return IServiceProvider
        _serviceScopeMock.SetupGet(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup IServiceProvider to return DatabaseContext
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DatabaseContext)))
            .Returns(_dbContextMock.Object);

        // Setup DatabaseContext to return DbSet<UserSignatureKeyPair>
        _dbContextMock.Setup(c => c.UserSignatureKeyPairs)
            .Returns(_dbSetMock.Object);

        // Setup IMapper (empty config)
        var mapperConfig = new MapperConfiguration(cfg => { });
        _mapper = mapperConfig.CreateMapper();
    }

    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScope_AndUpdatesEntity()
    {
        // Arrange
        var repo = new UserSignatureKeyPairRepository(_serviceScopeFactoryMock.Object, _mapper);

        var grantorId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "alg",
            WrappedSigningKey = new byte[] { 1, 2, 3 },
            VerifyingKey = new byte[] { 4, 5, 6 }
        };

        var entity = new UserSignatureKeyPair
        {
            UserId = grantorId,
            SignatureAlgorithm = "oldAlg",
            SigningKey = new byte[] { 0 },
            VerifyingKey = new byte[] { 0 }
        };

        // Setup FirstOrDefaultAsync to return the entity when queried with matching UserId
        _dbSetMock.Setup(d => d.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<UserSignatureKeyPair, bool>>>(),
            default))
            .ReturnsAsync(entity);

        // Setup SaveChangesAsync to return 1
        _dbContextMock.Setup(c => c.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        var updateFunc = repo.UpdateForKeyRotation(grantorId, signingKeys);
        await updateFunc(null, null);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbSetMock.Verify(d => d.FirstOrDefaultAsync(It.IsAny<Expression<Func<UserSignatureKeyPair, bool>>>(), default), Times.Once);
        _dbContextMock.Verify(c => c.SaveChangesAsync(default), Times.Once);

        Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
        Assert.True((DateTime.UtcNow - entity.RevisionDate).TotalSeconds < 5);
    }
}
