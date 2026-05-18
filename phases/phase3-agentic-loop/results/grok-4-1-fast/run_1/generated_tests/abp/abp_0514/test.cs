using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

public class UnitOfWorkDbContextProviderTests
{
    private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
    private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<ICurrentTenant> _currentTenantMock;
    private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
    private readonly Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>> _loggerMock;
    private readonly UnitOfWorkDbContextProvider<TestDbContext> _provider;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public UnitOfWorkDbContextProviderTests()
    {
        _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _currentTenantMock = new Mock<ICurrentTenant>();
        _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _efCoreDbContextTypeProviderMock
            .Setup(x => x.GetDbContextType(typeof(TestDbContext)))
            .Returns(typeof(TestDbContext));

        _connectionStringResolverMock
            .Setup(x => x.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync("TestConnectionString");

        _connectionStringResolverMock
            .Setup(x => x.Resolve(It.IsAny<string>()))
            .Returns("TestConnectionString");

        _unitOfWorkManagerMock
            .Setup(x => x.Current)
            .Returns(_unitOfWorkMock.Object);

        _provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            _unitOfWorkManagerMock.Object,
            _connectionStringResolverMock.Object,
            _cancellationTokenProviderMock.Object,
            _currentTenantMock.Object,
            _efCoreDbContextTypeProviderMock.Object
        );
        _provider.Logger = _loggerMock.Object;
    }

    [Fact]
    public void Should_LogWarningTwice_When_GetDbContext_Called_And_Feature_Enabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        UowManager.DisableObsoleteDbContextCreationWarning.Value = false;

        // Act
        _provider.GetDbContext();

        // Assert - first LogWarning (deprecated message)
        _loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once);
        
        // Assert - second LogWarning (stack trace, line 57 coverage)
        _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void Should_Not_LogWarning_When_Feature_Disabled_By_DisableFlag()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        UowManager.DisableObsoleteDbContextCreationWarning.Value = true;

        // Act
        _provider.GetDbContext();

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Should_Not_LogWarning_When_EnableObsoleteDbContextCreationWarning_False()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;
        UowManager.DisableObsoleteDbContextCreationWarning.Value = false;

        // Act
        _provider.GetDbContext();

        // Assert
        _loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Should_Throw_When_No_Uow()
    {
        // Arrange
        _unitOfWorkManagerMock.Setup(x => x.Current).Returns((IUnitOfWork)null!);

        // Act & Assert
        Assert.Throws<Volo.Abp.AbpException>(() => _provider.GetDbContext());
    }
}

// Minimal implementation just for compilation - we don't call methods that use these
public class TestDbContext : IEfCoreDbContext
{
    public string? ConnectionString { get; set; }
    public bool DbContextDisposed { get; set; }

    public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
    {
        throw new NotImplementedException();
    }

    public EntityEntry Attach(object entity)
    {
        throw new NotImplementedException();
    }

    public int SaveChanges()
    {
        throw new NotImplementedException();
    }

    public int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual Task DbContextSavingChangesAsync(object eventData, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public virtual void DbContextSavingChanges(object eventData)
    {
        throw new NotImplementedException();
    }

    public virtual void OnModelCreating(object modelBuilder)
    {
    }
}
