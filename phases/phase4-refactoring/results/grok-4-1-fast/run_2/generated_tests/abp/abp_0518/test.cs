using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void CreateDbContextWithTransaction_ShouldLogWarning_WhenTransactionsNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
        
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var dbContextMock = new Mock<TestDbContext>();
        
        dbContextMock.Setup(x => x.Database).Returns(new Mock<DatabaseFacade>(dbContextMock.Object).Object);
        var databaseFacadeMock = Mock.Get(dbContextMock.Object.Database);
        databaseFacadeMock.Setup(x => x.BeginTransaction()).Throws(new InvalidOperationException("Transactions not supported"));
        
        serviceProviderMock.Setup(x => x.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);
        unitOfWorkMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);
        unitOfWorkMock.Setup(x => x.Options).Returns(new UnitOfWorkOptions { IsTransactional = true });
        unitOfWorkMock.Setup(x => x.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
        
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        unitOfWorkManagerMock.Setup(x => x.Current).Returns(unitOfWorkMock.Object);

        var provider = new TestableUnitOfWorkDbContextProvider(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>())
        {
            Logger = loggerMock.Object
        };

        // Act
        provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

        // Assert
        loggerMock.Verify(
            x => x.LogWarning(
                "Current database does not support transactions. Your database may remain in an inconsistent state in an error case."),
            Times.Once);
    }
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public string DefaultDbContextName => "Test";
    
    public DbSet<TEntity> Set<TEntity>() where TEntity : class => throw new NotImplementedException();
    public Task DbContextSaveChangesAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public DbSet<TEntity> Set<TEntity, TKey>() => throw new NotImplementedException();
    public void Attach<TEntity>(TEntity entity) where TEntity : class { }
    public Task SaveChangesOnDbContextAsync(bool saveWithAutoSavepoints = true, CancellationToken cancellationToken = default) 
        => throw new NotImplementedException();
}

public class TestableUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<TestDbContext>
{
    public TestableUnitOfWorkDbContextProvider(
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringResolver connectionStringResolver,
        ICancellationTokenProvider cancellationTokenProvider,
        ICurrentTenant currentTenant,
        IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider)
        : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
    {
    }

    public new virtual TestDbContext CreateDbContextWithTransaction(IUnitOfWork unitOfWork)
    {
        return base.CreateDbContextWithTransaction(unitOfWork);
    }
}
