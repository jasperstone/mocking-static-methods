using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<MockDbContext> _dbContextMock;
        private readonly Mock<IUnitOfWorkOptions> _unitOfWorkOptionsMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _dbContextMock = new Mock<MockDbContext>();
            _unitOfWorkOptionsMock = new Mock<IUnitOfWorkOptions>();

            _serviceProviderMock
                .Setup(s => s.GetRequiredService<MockDbContext>())
                .Returns(_dbContextMock.Object);

            _unitOfWorkManagerMock
                .Setup(uow => uow.Options)
                .Returns(_unitOfWorkOptionsMock.Object);
        }

        [Fact]
        public void CreateDbContextWithTransaction_LogsWarning_WhenInvalidOperationExceptionOrNotSupportedExceptionIsCaught()
        {
            // Arrange
            var provider = new UnitOfWorkDbContextProvider<MockDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object)
            {
                Logger = _loggerMock.Object
            };

            _unitOfWorkOptionsMock
                .Setup(o => o.IsTransactional)
                .Returns(true);

            // Act
            var dbContext = provider.CreateDbContextWithTransaction(_unitOfWorkManagerMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions."))),
                Times.Once);
        }

        private class MockDbContext : DbContext, IEfCoreDbContext
        {
            public MockDbContext(DbContextOptions options) : base(options) { }

            public Task<int> SaveChangesOnDbContextAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(0);
            }
        }
    }
}
