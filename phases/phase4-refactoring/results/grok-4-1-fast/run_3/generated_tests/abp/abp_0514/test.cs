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
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_ShouldLogTwoWarnings_WhenObsoleteWarningEnabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

        var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
        connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("TestConn");

        var efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>()))
            .Returns(typeof(TestDbContext));

        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        loggerMock.Setup(x => x.LogWarning(It.IsAny<string>())).Verifiable();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService<TestDbContext>()).Returns(new TestDbContext());

        var unitOfWorkOptionsMock = new Mock<IOptions<UnitOfWorkDefaultOptions>>();
        unitOfWorkOptionsMock.Setup(o => o.Value).Returns(new UnitOfWorkDefaultOptions { IsTransactional = false });
        unitOfWorkMock.Setup(u => u.Options).Returns(unitOfWorkOptionsMock.Object);
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            connectionStringResolverMock.Object,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            efCoreDbContextTypeProviderMock.Object);

        // Set logger via reflection
        typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetProperty("Logger")!
            .SetValue(provider, loggerMock.Object);

        // Enable warning
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        // Act
        provider.GetDbContext();

        // Assert - specifically covers line 57 (second LogWarning with stack trace)
        loggerMock.Verify(
            x => x.LogWarning(
                It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
            Times.Once()
        );
        loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_ShouldNotLogWarning_WhenObsoleteWarningDisabled()
    {
        // Arrange
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(Mock.Of<IUnitOfWork>());

        var loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();

        var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>());

        typeof(UnitOfWorkDbContextProvider<TestDbContext>)
            .GetProperty("Logger")!
            .SetValue(provider, loggerMock.Object);

        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        // Act
        provider.GetDbContext();

        // Assert
        loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Never());
    }
}

public class TestDbContext : DbContext, IEfCoreDbContext
{
    public TestDbContext(DbContextOptions options) : base(options) { }

    public string? ConnectionString { get; set; }

    public Task DbContextSavedAsync(IReadOnlyList<EntityChangeEventData> entityChanges, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    // Minimal implementations for IEfCoreDbContext
    public new virtual EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
        => base.Attach(entity);

    public new virtual EntityEntry Attach(object entity)
        => base.Attach(entity);

    public new virtual int SaveChanges()
        => base.SaveChanges();

    public new virtual int SaveChanges(bool acceptAllChangesOnSuccess)
        => base.SaveChanges(acceptAllChangesOnSuccess);

    public new virtual Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    public new virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);

    public virtual Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    public new virtual DbSet<T> Set<T>() where T : class
        => base.Set<T>();

    public new virtual DbSet<T> Set<T>(string name) where T : class
        => base.Set<T>(name);

    public new virtual DatabaseFacade Database => base.Database;

    public new virtual ChangeTracker ChangeTracker => base.ChangeTracker;

    // Delegate other methods to base
    public new virtual EntityEntry Add(object entity) => base.Add(entity);
    public new virtual EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => base.Add(entity);
    public new virtual ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
        => base.AddAsync(entity, cancellationToken);
    public new virtual ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class => base.AddAsync(entity, cancellationToken);
    public new virtual void AddRange(IEnumerable<object> entities) => base.AddRange(entities);
    public new virtual void AddRange(params object[] entities) => base.AddRange(entities);
    public new virtual Task AddRangeAsync(params object[] entities) => base.AddRangeAsync(entities);
    public new virtual Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        => base.AddRangeAsync(entities, cancellationToken);
    public new virtual void AttachRange(IEnumerable<object> entities) => base.AttachRange(entities);
    public new virtual void AttachRange(params object[] entities) => base.AttachRange(entities);
    public new virtual EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class => base.Entry(entity);
    public new virtual EntityEntry Entry(object entity) => base.Entry(entity);
    public new virtual object? Find(Type entityType, params object[] keyValues) => base.Find(entityType, keyValues);
    public new virtual TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class => base.Find<TEntity>(keyValues);
    public new virtual ValueTask<object?> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
        => base.FindAsync(entityType, keyValues, cancellationToken);
    public new virtual ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken)
        where TEntity : class => base.FindAsync<TEntity>(keyValues, cancellationToken);
    public new virtual ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        => base.FindAsync<TEntity>(keyValues);
    public new virtual ValueTask<object?> FindAsync(Type entityType, params object[] keyValues)
        => base.FindAsync(entityType, keyValues);
    public new virtual EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class => base.Remove(entity);
    public new virtual EntityEntry Remove(object entity) => base.Remove(entity);
    public new virtual void RemoveRange(IEnumerable<object> entities) => base.RemoveRange(entities);
    public new virtual void RemoveRange(params object[] entities) => base.RemoveRange(entities);
    public new virtual EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class => base.Update(entity);
    public new virtual EntityEntry Update(object entity) => base.Update(entity);
    public new virtual void UpdateRange(params object[] entities) => base.UpdateRange(entities);
    public new virtual void UpdateRange(IEnumerable<object> entities) => base.UpdateRange(entities);
}
