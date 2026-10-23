using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private readonly Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>> _loggerMock;
        private readonly Mock<IUnitOfWorkManager> _unitOfWorkManagerMock;
        private readonly Mock<IConnectionStringResolver> _connectionStringResolverMock;
        private readonly Mock<ICancellationTokenProvider> _cancellationTokenProviderMock;
        private readonly Mock<ICurrentTenant> _currentTenantMock;
        private readonly Mock<IMongoDbContextTypeProvider> _dbContextTypeProviderMock;
        private readonly Mock<IAbpMongoClientFactory> _mongoClientFactoryMock;
        private readonly UnitOfWorkMongoDbContextProvider<MockDbContext> _provider;

        public UnitOfWorkMongoDbContextProviderTests()
        {
            _loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
            _unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            _connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            _cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            _currentTenantMock = new Mock<ICurrentTenant>();
            _dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            _mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            _provider = new UnitOfWorkMongoDbContextProvider<MockDbContext>(
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
        public async Task GetDbContextAsync_ShouldLogWarning_WhenUsingObsoleteMethod()
        {
            // Arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IMongoDbDatabaseApi>();
            var mockDbContext = new Mock<MockDbContext>();
            var mockServiceProvider = new ServiceCollection()
                .AddTransient<TMongoDbContext>(_ => mockDbContext.Object)
                .BuildServiceProvider();

            mockUnitOfWork.Setup(u => u.ServiceProvider).Returns(mockServiceProvider);
            mockUnitOfWork.Setup(u => u.Options).Returns(new UnitOfWorkOptions { IsTransactional = false });
            mockUnitOfWork.Setup(u => u.FindDatabaseApi(It.IsAny<string>())).Returns((IMongoDbDatabaseApi)null);
            mockUnitOfWork.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IMongoDbDatabaseApi>>()))
                .Returns((string key, Func<IMongoDbDatabaseApi> factory) => factory());

            _unitOfWorkManagerMock.Setup(u => u.Current).Returns(mockUnitOfWork.Object);
            _dbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(typeof(MockDbContext));
            _connectionStringResolverMock.Setup(r => r.ResolveConnectionString(It.IsAny<Type>())).Returns("mongodb://localhost/test");
            _mongoClientFactoryMock.Setup(c => c.GetAsync(It.IsAny<MongoUrl>())).ReturnsAsync(Mock.Of<IMongoClient>());

            // Act
            await _provider.GetDbContextAsync();

            // Assert
            _loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("deprecated"))),
                Times.AtLeastOnce
            );
        }

        // Additional tests can be added here to cover other code paths
    }

    public class MockDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session)
        {
            // Mock implementation
        }
    }
}
