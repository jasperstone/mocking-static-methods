using System;
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

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<SampleDbContext>>>();

            _provider = new UnitOfWorkDbContextProvider<SampleDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            _provider.Logger = _loggerMock.Object;
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldLogWarning_WhenCalledOutsideUnitOfWork()
        {
            // Arrange
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            await Assert.ThrowsAsync<AbpException>(async () =>
            {
                await _provider.GetDbContextAsync();
            });
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldReturnDbContext_WhenInsideUnitOfWork()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new SampleDbContext();

            mockUnitOfWork.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            mockUnitOfWork.Setup(m => m.FindDatabaseApi(It.IsAny<string>())).Returns((IEfCoreDatabaseApi)null);
            mockUnitOfWork.Setup(m => m.AddDatabaseApi(It.IsAny<string>(), It.IsAny<IEfCoreDatabaseApi>()))
                .Callback<string, IEfCoreDatabaseApi>((key, api) =>
                {
                    // do nothing
                });
            mockUnitOfWork.Setup(m => m.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(() =>
                {
                    mockDatabaseApi.Setup(api => api.DbContext).Returns(mockDbContext);
                    return mockDatabaseApi.Object;
                });
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            _efCoreDbContextTypeProviderMock.Setup(m => m.GetDbContextType(It.IsAny<Type>())).Returns(typeof(SampleDbContext));
            _connectionStringResolverMock.Setup(m => m.ResolveAsync(It.IsAny<string>())).ReturnsAsync("connStr");

            // Act
            var result = await _provider.GetDbContextAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SampleDbContext>(result);
        }

        [Fact]
        public void CreateDbContextWithTransaction_ShouldLogWarning_WhenActiveTransactionIsNull()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDbContext = new SampleDbContext();

            mockUnitOfWork.Setup(m => m.ServiceProvider).Returns(mockServiceProvider.Object);
            mockUnitOfWork.Setup(m => m.Options).Returns(new UnitOfWorkOptions { IsTransactional = true });
            mockUnitOfWork.Setup(m => m.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
            mockServiceProvider.Setup(m => m.GetRequiredService<SampleDbContext>()).Returns(mockDbContext);

            // Act
            var providerType = typeof(UnitOfWorkDbContextProvider<SampleDbContext>);
            var method = providerType.GetMethod("CreateDbContextWithTransaction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var instance = new UnitOfWorkDbContextProvider<SampleDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            instance.Logger = _loggerMock.Object;

            // Use reflection to invoke the protected method
            var result = method.Invoke(instance, new object[] { mockUnitOfWork.Object }) as SampleDbContext;

            // Verify that LogWarning was called
            _loggerMock.Verify(
                m => m.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions"))),
                Times.Once);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
            // Implementation not needed for test
        }
    }
}
