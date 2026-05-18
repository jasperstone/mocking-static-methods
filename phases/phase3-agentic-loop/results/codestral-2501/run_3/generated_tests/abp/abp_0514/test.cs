using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore;
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
        public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
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
            _connectionStringResolverMock.Setup(c => c.ResolveConnectionString(It.IsAny<string>())).Returns("TestConnectionString");

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead!")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldNotThrow_WhenUnitOfWorkIsNull()
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

        public EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public EntityEntry Attach(object entity)
        {
            throw new NotImplementedException();
        }
    }
}
