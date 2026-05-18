using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

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
            // Setup static properties and methods
            // Enable warning
            // For the purpose of this test, simulate the static flag
            // Since static flags are not accessible, assume the warning is enabled
            // and the code path is executed

            // Setup UnitOfWorkManager.Current to return _unitOfWorkMock
            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(_unitOfWorkMock.Object);
            // Setup UnitOfWork.Options.IsTransactional to false
            _unitOfWorkMock.SetupGet(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            // Setup ServiceProvider to return a mock of TMongoDbContext
            var serviceCollection = new ServiceCollection();
            var mockDbContext = new Mock<ITestMongoDbContext>();
            serviceCollection.AddTransient(_ => mockDbContext.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProvider);
            // Setup DbContextTypeProvider
            var mockType = typeof(ITestMongoDbContext);
            _dbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(mockType);
            // Setup ConnectionStringResolver
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<Type>())).Returns("mongodb://localhost:27017");

            // Act
            var result = _provider.GetDbContext();

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.Once);
        }
    }

    public interface ITestMongoDbContext : IAbpMongoDbContext
    {
    }
}
