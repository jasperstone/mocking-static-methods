using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>> _loggerMock;
        private readonly UnitOfWorkDbContextProvider<SampleDbContext> _provider;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly SampleDbContext _dbContext;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dbContext = new SampleDbContext();

            _provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenUsingObsoleteMethod()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(_unitOfWorkMock.Object);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(typeof(SampleDbContext));
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("connStr");
            _unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns((string key, Func<IEfCoreDatabaseApi> factory) => new EfCoreDatabaseApi(_dbContext));

            // Act
            var context = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("deprecated"))),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldReturnDbContext()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(_unitOfWorkMock.Object);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(typeof(SampleDbContext));
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).ReturnsAsync("connStr");
            _unitOfWorkMock.Setup(u => u.FindDatabaseApi(It.IsAny<string>())).Returns((IEfCoreDatabaseApi)null);
            _unitOfWorkMock.Setup(u => u.AddDatabaseApi(It.IsAny<string>(), It.IsAny<IEfCoreDatabaseApi>()))
                .Callback<string, IEfCoreDatabaseApi>((key, api) => { });

            // Act
            var result = await _provider.GetDbContextAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SampleDbContext>(result);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
        }
    }
}
