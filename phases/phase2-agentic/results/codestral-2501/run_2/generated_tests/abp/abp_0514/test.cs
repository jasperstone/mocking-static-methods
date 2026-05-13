using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.EntityFrameworkCore;
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
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>> _loggerMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<IEfCoreDbContext>>>();
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _unitOfWorkManagerMock.Setup(u => u.DisableObsoleteDbContextCreationWarning).Returns(false);
            UnitOfWork.EnableObsoleteDbContextCreationWarning = true;

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public void GetDbContext_ShouldThrowAbpException_WhenNoUnitOfWork()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            Assert.Throws<AbpException>(() => unitOfWorkDbContextProvider.GetDbContext());
        }

        [Fact]
        public async Task GetDbContextAsync_ShouldThrowAbpException_WhenNoUnitOfWork()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<IEfCoreDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns((IUnitOfWork)null);

            // Act & Assert
            await Assert.ThrowsAsync<AbpException>(() => unitOfWorkDbContextProvider.GetDbContextAsync());
        }
    }
}
