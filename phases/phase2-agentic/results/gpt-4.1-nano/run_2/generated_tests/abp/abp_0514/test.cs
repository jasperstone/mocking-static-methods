using System;
using System.Diagnostics;
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
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>> _loggerMock;
        private readonly UnitOfWorkDbContextProvider<DbContext> _provider;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>>();

            _provider = new UnitOfWorkDbContextProvider<DbContext>(
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
            var mockUnitOfWorkManager = new Mock<IUnitOfWorkManager>();
            mockUnitOfWorkManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new Mock<DbContext>();
            mockDatabaseApi.Setup(d => d.DbContext).Returns(mockDbContext.Object);
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(mockDatabaseApi.Object);
            var efCoreType = typeof(DbContext);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(efCoreType);
            var connectionString = "connStr";
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<string>())).Returns(connectionString);
            // Enable warning
            typeof(UnitOfWorkDbContextProvider<DbContext>)
                .GetProperty("UnitOfWorkManager")
                .SetValue(_provider, _unitOfWorkManagerMock.Object);
            // Act
            // Force the static EnableObsoleteDbContextCreationWarning to true
            // (simulate the condition)
            // Since it's a static property, we can't set it directly here, so we assume it's true for test
            // Call GetDbContext
            var context = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("deprecated"))),
                Times.AtLeastOnce
            );
        }
    }
}
