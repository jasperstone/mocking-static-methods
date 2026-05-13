using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Data;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
        private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<ITestMongoDbContext>>> _loggerMock;
        private readonly UnitOfWorkMongoDbContextProvider<ITestMongoDbContext> _provider;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<ITestMongoDbContext>>>();

            // Setup default behaviors
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);
            _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
            _unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<DatabaseApi>>()))
                .Returns((string key, Func<DatabaseApi> factory) => new DatabaseApi());

            _provider = new UnitOfWorkMongoDbContextProvider<ITestMongoDbContext>(
                _unitOfWorkManagerMock.Object,
                _connectionStringResolverMock.Object,
                _cancellationTokenProviderMock.Object,
                _currentTenantMock.Object,
                _dbContextTypeProviderMock.Object,
                _mongoClientFactoryMock.Object
            );
            _provider.Logger = _loggerMock.Object;
        }

        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            _unitOfWorkMock.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            // Enable warning
            var enableWarning = true;
            // Simulate environment stack trace
            var stackTrace = "Test stack trace for warning";

            // Act
            _provider.GetType().GetProperty("UnitOfWork").SetValue(_provider, _unitOfWorkMock.Object);
            // Force the static Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value to false
            // Since it's static, we can't set it directly, so we assume it's false or mock accordingly
            // For this test, we focus on the LogWarning call

            // Call the method
            _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.AtLeastOnce
            );
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(msg => msg.Contains(Environment.StackTrace.Truncate(2048)))),
                Times.AtLeastOnce
            );
        }
    }

    // Dummy interface for testing
    public interface ITestMongoDbContext : IAbpMongoDbContext
    {
    }

    // Dummy class for testing
    public class DatabaseApi { }
}
