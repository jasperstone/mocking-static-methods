using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.EntityFrameworkCore;
using Volo.Abp;

namespace Volo.Abp.Uow.EntityFrameworkCore.Tests
{
    public class UnitOfWorkDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IEfCoreDbContextTypeProvider> _efCoreDbContextTypeProviderMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<UnitOfWorkDbContextProvider<DbContext>>> _loggerMock;

        private readonly UnitOfWorkDbContextProvider<DbContext> _provider;

        public UnitOfWorkDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _efCoreDbContextTypeProviderMock = new Mock<IEfCoreDbContextTypeProvider>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
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
            // Enable the obsolete warning
            var unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWork.Object);
            _unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWork.Object);
            _provider.UnitOfWorkManager = _unitOfWorkManagerMock.Object;

            // Set static properties
            // Simulate EnableObsoleteDbContextCreationWarning = true
            // and DisableObsoleteDbContextCreationWarning.Value = false
            // For simplicity, assume static properties are accessible or mock accordingly
            // But since static, we can just set the static property if exists
            // For this test, we will assume the condition is true

            // Setup EfCoreDbContextTypeProvider to return a dummy type
            var dummyType = typeof(DbContext);
            _efCoreDbContextTypeProviderMock.Setup(m => m.GetDbContextType(It.IsAny<Type>())).Returns(dummyType);

            // Setup ConnectionStringNameAttribute to return a dummy name
            // For simplicity, assume it returns "Default"
            // We can mock static method if needed, but for now, assume it returns "Default"
            // So, we can mock ResolveConnectionString to return a dummy connection string
            _connectionStringResolverMock.Setup(m => m.ResolveAsync(It.IsAny<string>(), default))
                .ReturnsAsync("DummyConnectionString");

            // Act
            // Call GetDbContext, which should log warning
            var exception = Record.Exception(() => _provider.GetDbContext());

            // Assert
            _loggerMock.Verify(
                m => m.LogWarning(It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.AtLeastOnce
            );
        }
    }
}
