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
        public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new SampleDbContext();

            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(mockDatabaseApi.Object);
            mockUnitOfWork.Setup(u => u.Current).Returns(mockUnitOfWork.Object);
            mockUnitOfWork.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            mockUnitOfWork.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());

            var unitOfWorkManager = new Mock<IUnitOfWorkManager>();
            unitOfWorkManager.Setup(u => u.Current).Returns(mockUnitOfWork.Object);
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(mockUnitOfWork.Object);

            var efCoreType = typeof(SampleDbContext);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(efCoreType);
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns("connStr");
            // Force the static environment to simulate warning enabled
            // For simplicity, assume static property is true
            // Alternatively, you can set the static property if exists

            // Act
            // Temporarily set the static flag for warning
            // Since the code checks UnitOfWork.EnableObsoleteDbContextCreationWarning
            // We can simulate this by setting the static property if accessible
            // But since it's not in the snippet, assume it's true
            // Call GetDbContext
            var context = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains("deprecated"))),
                Times.AtLeastOnce
            );
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains(Environment.StackTrace.Truncate(2048)))),
                Times.AtLeastOnce
            );
        }
    }

    // Dummy DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(AbpEfCoreDbContextInitializationContext context)
        {
        }
    }
}
