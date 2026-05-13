using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;

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
            var stackTrace = "Sample stack trace for testing purposes.";
            Environment.SetEnvironmentVariable("STACK_TRACE", stackTrace);

            // Act
            var exception = Record.Exception(() => _provider.GetDbContext());

            // Assert
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.Once);
            _loggerMock.Verify(
                log => log.LogWarning(It.Is<string>(s => s.Contains(Environment.StackTrace.Truncate(2048)))),
                Times.Once);
        }
    }

    // Dummy implementations for missing types
    public class ITestMongoDbContext : IAbpMongoDbContext { }
    public class DatabaseApi { }
    public class UnitOfWorkOptions
    {
        public bool IsTransactional { get; set; }
        public TimeSpan? Timeout { get; set; }
    }
}
