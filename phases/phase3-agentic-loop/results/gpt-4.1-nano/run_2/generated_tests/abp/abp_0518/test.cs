using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

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
        public void GetDbContext_Warns_WhenObsoleteAndInsideUow()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new Mock<SampleDbContext>();
            var stackTrace = new System.Diagnostics.StackTrace().ToString();

            mockUow.Setup(u => u.Options.IsTransactional).Returns(false);
            mockUow.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            mockUow.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(() => new EfCoreDatabaseApi(mockDbContext.Object));
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(mockUow.Object);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(SampleDbContext)))
                .Returns(typeof(SampleDbContext));
            _connectionStringResolverMock.Setup(r => r.ResolveAsync(It.IsAny<string>()))
                .ReturnsAsync("connStr");
            _loggerMock.Setup(l => l.LogWarning(It.IsAny<string>()));

            // Act
            var result = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetDbContextAsync_CreatesAndReturnsDbContext()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockDbContext = new Mock<SampleDbContext>();
            var connectionString = "connStr";

            mockUow.Setup(u => u.Options.IsTransactional).Returns(false);
            mockUow.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            mockUow.Setup(u => u.FindDatabaseApi(It.IsAny<string>())).Returns((IEfCoreDatabaseApi)null);
            mockUow.Setup(u => u.AddDatabaseApi(It.IsAny<string>(), It.IsAny<IEfCoreDatabaseApi>()));
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(mockUow.Object);
            _efCoreDbContextTypeProviderMock.Setup(p => p.GetDbContextType(typeof(SampleDbContext)))
                .Returns(typeof(SampleDbContext));
            _connectionStringResolverMock.Setup(r => r.ResolveAsync(It.IsAny<string>()))
                .ReturnsAsync(connectionString);

            // Act
            var dbContext = await _provider.GetDbContextAsync();

            // Assert
            Assert.NotNull(dbContext);
        }
    }

    // Sample DbContext for testing
    public class SampleDbContext : DbContext, IEfCoreDbContext
    {
        public void Initialize(object context) { }
    }
}
