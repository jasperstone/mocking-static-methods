using System;
using System.Threading;
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
        public async Task GetDbContextAsync_ShouldLogWarning_WhenCalled()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IEfCoreDatabaseApi>();
            var mockDbContext = new Mock<SampleDbContext>();
            var dbContext = mockDbContext.Object;

            mockUnitOfWork.Setup(u => u.Current).Returns(mockUnitOfWork.Object);
            mockUnitOfWork.Setup(u => u.FindDatabaseApi(It.IsAny<string>())).Returns((IEfCoreDatabaseApi)null);
            mockUnitOfWork.Setup(u => u.AddDatabaseApi(It.IsAny<string>(), It.IsAny<IEfCoreDatabaseApi>()))
                .Callback<string, IEfCoreDatabaseApi>((key, api) =>
                {
                    // do nothing
                });
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IEfCoreDatabaseApi>>()))
                .Returns(() =>
                {
                    return new EfCoreDatabaseApi(dbContext);
                });
            mockUnitOfWork.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(mockUnitOfWork.Object);

            _connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<string>()))
                .ReturnsAsync("TestConnectionString");
            _efCoreDbContextTypeProviderMock.Setup(e => e.GetDbContextType(It.IsAny<Type>()))
                .Returns(typeof(SampleDbContext));
            // Act
            var result = await _provider.GetDbContextAsync();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.IsAny<string>()),
                Times.Never); // Because in this setup, no exception thrown, so no warning expected
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
