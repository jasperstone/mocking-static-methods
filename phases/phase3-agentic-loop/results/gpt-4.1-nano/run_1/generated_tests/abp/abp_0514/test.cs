using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.Uow;
using Volo.Abp.DependencyInjection;

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
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabledAndNotDisabled()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUnitOfWorkManager = _unitOfWorkManagerMock;
            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            mockUnitOfWork.SetupGet(u => u.EnableObsoleteDbContextCreationWarning).Returns(true);
            // Simulate that DisableObsoleteDbContextCreationWarning.Value is false
            // For this, we need to mock the static Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning
            // but since static properties are hard to mock, we will assume the static is false for this test
            // Alternatively, we can set the static property directly if accessible

            // Mock EfCoreDbContextTypeProvider
            var dbContextType = typeof(SampleDbContext);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(dbContextType);

            // Mock ConnectionStringNameAttribute
            var connectionStringName = "Default";
            // Mock ResolveConnectionString
            _connectionStringResolverMock.Setup(r => r.ResolveAsync(It.IsAny<string>(), default))
                .ReturnsAsync("Server=.;Database=TestDb;Trusted_Connection=True;");

            // Setup environment for GetDbContext
            // We need to set UnitOfWorkManager.Current to a valid object
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new Mock<SampleDbContext>();
            mockDatabaseApi.SetupGet(a => a.DbContext).Returns(mockDbContext.Object);
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(mockDatabaseApi.Object);
            mockUnitOfWork.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            mockUnitOfWork.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());

            // Act
            _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.AtLeastOnce);
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
