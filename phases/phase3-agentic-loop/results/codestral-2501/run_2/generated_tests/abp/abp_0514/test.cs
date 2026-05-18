using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.EntityFrameworkCore;
using Xunit;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>> _loggerMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenObsoleteMethodIsCalled()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _unitOfWorkManagerMock.Setup(u => u.DisableObsoleteDbContextCreationWarning).Returns(new AsyncLocal<bool>(() => false));
            _efCoreDbContextTypeProviderMock.Setup(e => e.GetDbContextType(typeof(TestDbContext))).Returns(typeof(TestDbContext));

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldNotThrow_WhenCalledInsideUnitOfWork()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _efCoreDbContextTypeProviderMock.Setup(e => e.GetDbContextType(typeof(TestDbContext))).Returns(typeof(TestDbContext));

            // Act
            var result = await unitOfWorkDbContextProvider.GetDbContextAsync();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetDbContext_ShouldThrowAbpException_WhenNotInsideUnitOfWork()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            Assert.Throws<AbpException>(() => unitOfWorkDbContextProvider.GetDbContext());
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldThrowAbpException_WhenNotInsideUnitOfWork()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            await Assert.ThrowsAsync<AbpException>(() => unitOfWorkDbContextProvider.GetDbContextAsync());
        }
    }

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
            // Initialization logic
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Entry(object entity)
        {
            throw new NotImplementedException();
        }

        public int SaveChanges()
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

        public DatabaseFacade Database => throw new NotImplementedException();

        public ChangeTracker ChangeTracker => throw new NotImplementedException();

        public EntityEntry Attach<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Attach(object entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public DbSet<T> Set<T>(string name) where T : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Add(object entity)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void AddRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void AddRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void AttachRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void AttachRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public object? Find(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public TEntity? Find<TEntity>(params object[] keyValues) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<object?> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask<TEntity?> FindAsync<TEntity>(object[] keyValues, CancellationToken cancellationToken) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public ValueTask<object?> FindAsync(Type entityType, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Remove<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Remove(object entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }

        public void RemoveRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Update(object entity)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange(params object[] entities)
        {
            throw new NotImplementedException();
        }

        public void UpdateRange(IEnumerable<object> entities)
        {
            throw new NotImplementedException();
        }
    }
}
