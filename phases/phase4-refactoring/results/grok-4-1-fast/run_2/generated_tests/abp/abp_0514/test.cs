using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore.Uow;

public class UnitOfWorkDbContextProviderTests
{
    [Fact]
    public void GetDbContext_Should_LogWarning_When_ObsoleteWarningEnabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = true;
        Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value = false;

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Options).Returns(new UnitOfWorkOptions());
        unitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
            .Returns(new Mock<EfCoreDatabaseApi>().Object);

        var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
        unitOfWorkManager.Setup(m => m.Current).Returns(unitOfWork.Object);

        var efCoreDbContextTypeProvider = new Mock<IEfCoreDbContextTypeProvider>();
        efCoreDbContextTypeProvider.Setup(p => p.GetDbContextType(It.IsAny<Type>()))
            .Returns(typeof(MockDbContext));

        var connectionStringResolver = new Mock<IConnectionStringResolver>();
        connectionStringResolver.Setup(r => r.ResolveAsync(It.IsAny<string>()))
            .ReturnsAsync("TestConnection");

        var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>>();
        SetupLoggerForDeprecationWarning(logger);

        var provider = new TestableUnitOfWorkDbContextProvider<MockDbContext>(
            unitOfWorkManager.Object,
            connectionStringResolver.Object,
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            efCoreDbContextTypeProvider.Object)
        {
            Logger = logger.Object
        };

        // Act
        provider.GetDbContext();

        // Assert
        logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);

        logger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Exactly(2));
    }

    [Fact]
    public void GetDbContext_Should_Not_LogWarning_When_ObsoleteWarningDisabled()
    {
        // Arrange
        UnitOfWork.EnableObsoleteDbContextCreationWarning = false;

        var unitOfWork = new Mock<IUnitOfWork>();
        var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
        unitOfWorkManager.Setup(m => m.Current).Returns(unitOfWork.Object);

        var logger = new Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>>();

        var provider = new TestableUnitOfWorkDbContextProvider<MockDbContext>(
            unitOfWorkManager.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IEfCoreDbContextTypeProvider>())
        {
            Logger = logger.Object
        };

        // Act
        provider.GetDbContext();

        // Assert
        logger.Verify(l => l.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    private static void SetupLoggerForDeprecationWarning(Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>> logger)
    {
        logger.Setup(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));
    }
}

public class TestableUnitOfWorkDbContextProvider<TDbContext> : UnitOfWorkDbContextProvider<TDbContext>
    where TDbContext : IEfCoreDbContext
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

    public new ILogger<UnitOfWorkDbContextProvider<TDbContext>> Logger
    {
        get => base.Logger;
        set => base.Logger = value;
    }

    protected internal new virtual string ResolveConnectionString(string connectionStringName) => "TestConnection";

    protected internal new virtual TDbContext CreateDbContext(IUnitOfWork unitOfWork, string connectionStringName, string connectionString)
        => (TDbContext)Activator.CreateInstance(typeof(MockDbContext))!;
}

public class MockDbContext : DbContext, IEfCoreDbContext
{
    public string? ConnectionString { get; set; }

    public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
    public EntityEntry Attach(object entity) => throw new NotImplementedException();
    public int SaveChanges() => 0;
    public int SaveChanges(bool acceptAllChangesOnSuccess) => 0;
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => Task.FromResult(0);
    public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) => Task.FromResult(0);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    public DbSet<TEntity> Set<TEntity>() where TEntity : class => Set<TEntity>();
    public DbSet<TEntity> Set<TEntity>(string name) where TEntity : class => Set<TEntity>();
    public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class => throw new NotImplementedException();
    public EntityEntry Add(object entity) => throw new NotImplementedException();
    public Task<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        => throw new NotImplementedException();
    public Task<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
