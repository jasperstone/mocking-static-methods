using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests : IDisposable
{
    private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
    private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
    private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
    private readonly Mock<ICurrentTenant> _currentTenantMock;
    private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>> _loggerMock;

    public UnitOfWorkDbContextProviderTests()
    {
        _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
        _currentTenantMock = new Mock<ICurrentTenant>();
        _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        
        _loggerMock.SetupAllProperties();
    }

    [Fact]
    public void CreateDbContextWithTransaction_ShouldLogWarning_WhenTransactionNotSupported()
    {
        // Arrange
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var options = new UnitOfWorkOptions { IsTransactional = true };
        unitOfWorkMock.Setup(u => u.Options).Returns(options);
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(_serviceProviderMock.Object);

        var dbContextMock = new Mock<TestDbContext>();
        dbContextMock.Setup(x => x.Database.BeginTransaction(It.IsAny<IsolationLevel>()))
            .Throws(new InvalidOperationException("Transactions not supported"));
        dbContextMock.Setup(x => x.Database.BeginTransaction())
            .Throws(new InvalidOperationException("Transactions not supported"));

        _serviceProviderMock.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            _unitOfWorkManagerMock.Object,
            _connectionStringResolverMock.Object,
            _cancellationTokenProviderMock.Object,
            _currentTenantMock.Object,
            _efCoreDbContextTypeProviderMock.Object
        );
        provider.Logger = _loggerMock.Object;

        // Act
        var result = provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString().Contains("Current database does not support transactions")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _unitOfWorkManagerMock?.Dispose();
        _loggerMock?.Dispose();
    }
}

public interface ITestDbContext : IEfCoreDbContext
{
}

public class TestDbContext : DbContext, ITestDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public int SaveChangesOnDbContext(bool acceptAllChangesOnSuccess)
    {
        throw new NotImplementedException();
    }
}
