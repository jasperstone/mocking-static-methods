using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IDbContextTransaction> _dbContextTransactionMock;
        private readonly Mock<TestDbContext> _dbContextMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _dbContextTransactionMock = new Mock<IDbContextTransaction>();
            _dbContextMock = new Mock<TestDbContext>();
        }

        [Fact]
        public void CreateDbContextWithTransaction_WhenActiveTransactionIsNull_LogsWarning()
        {
            // Arrange
            var provider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            )
            {
                Logger = _loggerMock.Object
            };

            _unitOfWorkMock.Setup(uow => uow.FindTransactionApi(It.IsAny<string>())).Returns((EfCoreTransactionApi)null);
            _unitOfWorkMock.Setup(uow => uow.ServiceProvider.GetRequiredService<TestDbContext>()).Returns(_dbContextMock.Object);
            _dbContextMock.Setup(db => db.Database.BeginTransaction(It.IsAny<IsolationLevel>())).Throws<InvalidOperationException>();

            // Act
            var result = provider.CreateDbContextWithTransaction(_unitOfWorkMock.Object);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Func<It.IsAnyType, Exception, string>>(), It.IsAny<Exception>()), Times.Once);
            Assert.Equal(_dbContextMock.Object, result);
        }

        private class TestDbContext : DbContext, IEfCoreDbContext
        {
            public void Initialize(AbpEfCoreDbContextInitializationContext context)
            {
                // Initialization logic
            }
        }
    }
}
