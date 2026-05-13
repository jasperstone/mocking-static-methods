using Moq;
using Microsoft.Extensions.Logging;
using System;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Volo.Abp.Data;
using Volo.Abp.Threading;

namespace AbpTests.Uow.EntityFrameworkCore
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
        private readonly UnitOfWorkDbContextProvider<MockDbContext> _provider;

        public UnitOfWorkDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<MockDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();

            _provider = new UnitOfWorkDbContextProvider<MockDbContext>(
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
        public void CreateDbContextWithTransaction_LogsWarning_WhenTransactionNotSupported()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Options.IsTransactional).Returns(true);
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<MockDbContext>()).Returns(new MockDbContext());

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);

            // Act
            var dbContext = _provider.CreateDbContextWithTransaction(unitOfWorkMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogWarning(It.Is<string>(s => s.Contains("Current database does not support transactions."))),
                Times.Once
            );
        }
    }

    public class MockDbContext : DbContext
    {
        public MockDbContext() : base()
        {
        }
    }
}
