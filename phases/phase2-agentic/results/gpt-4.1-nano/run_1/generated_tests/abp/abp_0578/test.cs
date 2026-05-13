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

        public class TestMongoDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, MongoClient client, object options) { }
            public object ToAbpMongoDbContext() => this;
        }

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
        public void GetDbContext_WarningLogged_WhenObsoleteWarningEnabled()
        {
            // Arrange
            // Setup static properties and methods
            // Enable warning
            var unitOfWork = _unitOfWorkMock.Object;
            var options = new Mock<IUnitOfWorkOptions>();
            options.SetupGet(o => o.IsTransactional).Returns(false);
            unitOfWork.Options = options.Object;
            _unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWork);
            _unitOfWorkManagerMock.Setup(m => m.DisableObsoleteDbContextCreationWarning).Returns(false);
            _unitOfWorkManagerMock.Setup(m => m).Returns(unitOfWork);
            // Setup type provider
            var dbContextType = typeof(TestMongoDbContext);
            _dbContextTypeProviderMock.Setup(p => p.GetDbContextType(It.IsAny<Type>())).Returns(dbContextType);
            // Setup connection string resolver
            var connectionString = "mongodb://localhost:27017/testdb";
            _connectionStringResolverMock.Setup(r => r.Resolve(It.IsAny<Type>())).Returns(connectionString);
            // Setup MongoUrl
            var mongoUrl = new MongoUrl(connectionString);
            // Setup GetOrAddDatabaseApi to return a dummy
            var databaseApiMock = new Mock<IMongoDbDatabaseApi>();
            databaseApiMock.SetupGet(d => d.DbContext).Returns(new TestMongoDbContext());
            var databaseApi = databaseApiMock.Object;
            // Setup GetOrAddDatabaseApi
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IMongoDbDatabaseApi>>()))
                .Returns(databaseApi);
            _unitOfWorkMock.SetupGet(u => u).Returns(unitOfWorkMock.Object);
            // Act
            _provider.GetDbContext();
            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated"))),
                Times.AtLeastOnce);
            _loggerMock.Verify(
                x => x.LogWarning(It.Is<string>(s => s.Contains(Environment.StackTrace))),
                Times.AtLeastOnce);
        }
    }
}
