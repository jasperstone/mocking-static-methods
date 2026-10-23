using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_Should_LogWarningTwice_When_ObsoleteWarningEnabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

        var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
        mockConnectionStringResolver.Setup(x => x.ResolveAsync(It.IsAny<string>())).ReturnsAsync("test");

        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));

        var efCoreTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreTypeProviderMock.Setup(x => x.GetDbContextType(It.IsAny<Type>())).Returns(typeof(TestDbContext));

        var provider = new TestUnitOfWorkDbContextProvider(
            unitOfWorkManagerMock.Object,
            mockConnectionStringResolver.Object,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            efCoreTypeProviderMock.Object,
            loggerMock.Object
        );

        // Enable the obsolete warning (tests line 57 specifically)
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        // Act
        provider.GetDbContext();

        // Assert - first LogWarning (deprecated message)
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once()
        );
        
        // Assert - second LogWarning (line 57: Environment.StackTrace.Truncate(2048))
        loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();

        var efCoreTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreTypeProviderMock.Setup(x => x.GetDbContextType(It.IsAny<Type>())).Returns(typeof(TestDbContext));

        var provider = new TestUnitOfWorkDbContextProvider(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            efCoreTypeProviderMock.Object,
            loggerMock.Object
        );

        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = true;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Never());
    }
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<T> Set<T>() where T : class => throw new NotImplementedException();
    public DbSet<T> Set<T>(string name) where T : class => throw new NotImplementedException();
    public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    // IDbContextDependencies - minimal implementation
    public IDbSetSource SetSource => throw new NotImplementedException();
    public IEntityFinderFactory EntityFinderFactory => throw new NotImplementedException();
    public IQueryProvider QueryProvider => throw new NotImplementedException();
    public IStateManager StateManager => throw new NotImplementedException();
    public IChangeDetector ChangeDetector => throw new NotImplementedException();
    public IEntityGraphAttacher EntityGraphAttacher => throw new NotImplementedException();

    // IDbSetCache
    public IDbSetCache DbSetCache => throw new NotImplementedException();

    // IDbContextPoolable
    public void SetDbContextPool(DbContextPool pool) { }
    public bool IsInPool => false;

    public void Dispose() { }
    IServiceProvider IInfrastructure<IServiceProvider>.Instance => throw new NotImplementedException();
}

public class TestUnitOfWorkDbContextProvider : UnitOfWorkDbContextProvider<TestDbContext>
{
    public TestUnitOfWorkDbContextProvider(
        IUnitOfWorkManager unitOfWorkManager,
        IConnectionStringResolver connectionStringResolver,
        ICancellationTokenProvider cancellationTokenProvider,
        ICurrentTenant currentTenant,
        IEfCoreDbContextTypeProvider efCoreDbContextTypeProvider,
        ILogger<UnitOfWorkDbContextProvider<TestDbContext>> logger)
        : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, efCoreDbContextTypeProvider)
    {
        Logger = logger;
    }

    protected override TestDbContext CreateDbContext(IUnitOfWork unitOfWork) => new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
    protected override TestDbContext CreateDbContext(IUnitOfWork unitOfWork, string connectionStringName, string connectionString) => new TestDbContext(new DbContextOptionsBuilder<TestDbContext>().Options);
}
