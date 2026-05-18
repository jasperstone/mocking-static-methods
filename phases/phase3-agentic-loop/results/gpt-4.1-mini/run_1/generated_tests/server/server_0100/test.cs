using System;
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
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<DbSet<UserSignatureKeyPair>> _dbSetMock;
    private readonly IMapper _mapper;

    public UserSignatureKeyPairRepositoryTests()
    {
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _dbContextMock = new Mock<DbContext>();
        _dbSetMock = new Mock<DbSet<UserSignatureKeyPair>>();

        // Setup IServiceScopeFactory to return IServiceScope
        _serviceScopeFactoryMock.Setup(f => f.CreateAsyncScope())
            .Returns(_serviceScopeMock.Object);

        // Setup IServiceScope to return a service provider that returns the DbContext
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext)))
            .Returns(_dbContextMock.Object);
        _serviceScopeMock.Setup(s => s.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        // Setup DbContext to return DbSet
        _dbContextMock.Setup(c => c.Set<UserSignatureKeyPair>())
            .Returns(_dbSetMock.Object);

        // Setup mapper (using AutoMapper default config for simplicity)
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserSignatureKeyPair, UserSignatureKeyPair>().ReverseMap();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task UpdateForKeyRotation_CallsCreateAsyncScopeAndUpdatesEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var signingKeys = new SignatureKeyPairData
        {
            SignatureAlgorithm = "alg",
            WrappedSigningKey = new byte[] { 1, 2, 3 },
            VerifyingKey = new byte[] { 4, 5, 6 }
        };

        var entity = new UserSignatureKeyPair
        {
            UserId = userId,
            SignatureAlgorithm = "oldAlg",
            SigningKey = new byte[] { 7, 8, 9 },
            VerifyingKey = new byte[] { 10, 11, 12 }
        };

        // Setup DbSet FirstOrDefaultAsync to return entity
        _dbSetMock.Setup(d => d.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<UserSignatureKeyPair, bool>>>(),
                default))
            .ReturnsAsync(entity);

        // Setup SaveChangesAsync
        _dbContextMock.Setup(d => d.SaveChangesAsync(default))
            .ReturnsAsync(1);

        var repo = new UserSignatureKeyPairRepository(_serviceScopeFactoryMock.Object, _mapper);

        // Act
        var updateFunc = repo.UpdateForKeyRotation(userId, signingKeys);
        await updateFunc(null, null);

        // Assert
        _serviceScopeFactoryMock.Verify(f => f.CreateAsyncScope(), Times.Once);
        _dbSetMock.Verify(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserSignatureKeyPair, bool>>>(), default), Times.Once);
        _dbContextMock.Verify(d => d.SaveChangesAsync(default), Times.Once);
        Assert.Equal(signingKeys.SignatureAlgorithm, entity.SignatureAlgorithm);
        Assert.Equal(signingKeys.WrappedSigningKey, entity.SigningKey);
        Assert.Equal(signingKeys.VerifyingKey, entity.VerifyingKey);
    }
}
