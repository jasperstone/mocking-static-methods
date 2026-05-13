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
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>> _loggerMock;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _loggerMock = new Mock<ILogger<UnitOfWorkDbContextProvider<TestDbContext>>>();
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteDbContextCreationWarningIsEnabled()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(new Mock<IUnitOfWork>().Object);
            _efCoreDbContextTypeProviderMock.Setup(e => e.GetDbContextType(typeof(TestDbContext))).Returns(typeof(TestDbContext));
            _connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<string>())).ReturnsAsync("TestConnectionString");

            // Act
            unitOfWorkDbContextProvider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task GetDbContextAsync_ReturnsDbContext_WhenUnitOfWorkIsAvailable()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _efCoreDbContextTypeProviderMock.Object
            );
            unitOfWorkDbContextProvider.Logger = _loggerMock.Object;

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(unitOfWorkMock.Object);
            _efCoreDbContextTypeProviderMock.Setup(e => e.GetDbContextType(typeof(TestDbContext))).Returns(typeof(TestDbContext));
            _connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<string>())).ReturnsAsync("TestConnectionString");

            var dbContextMock = new Mock<TestDbContext>();
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<TestDbContext>()).Returns(dbContextMock.Object);

            // Act
            var result = await unitOfWorkDbContextProvider.GetDbContextAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestDbContext>(result);
        }

        [Fact]
        public void GetDbContext_ThrowsAbpException_WhenUnitOfWorkIsNull()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
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
        public async Task GetDbContextAsync_ThrowsAbpException_WhenUnitOfWorkIsNull()
        {
            // Arrange
            var unitOfWorkDbContextProvider = new UnitOfWorkDbContextProvider<TestDbContext>(
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

    public class TestDbContext : DbContext, IEfCoreDbContext
    {
    }
}
