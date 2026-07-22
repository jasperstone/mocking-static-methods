using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp;
using Volo.Abp.UnitOfWork;
using Volo.Abp.DependencyInjection;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public void GetDbContext_ShouldLogWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<SampleMongoDbContext>>>();
            var mockUowManager = new Mock<IUnitOfWorkManager>();
            var mockConnectionStringResolver = new Mock<IConnectionStringResolver>();
            var mockCancellationTokenProvider = new Mock<ICancellationTokenProvider>();
            var mockCurrentTenant = new Mock<ICurrentTenant>();
            var mockDbContextTypeProvider = new Mock<IMongoDbContextTypeProvider>();
            var mockMongoClientFactory = new Mock<IAbpMongoClientFactory>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockDatabaseApi = new Mock<IMongoDatabaseApi>();
            var mockServiceProvider = new ServiceCollection().BuildServiceProvider();

            // Setup provider with mocked logger
            var provider = new UnitOfWorkMongoDbContextProvider<SampleMongoDbContext>(
                mockUowManager.Object,
                mockConnectionStringResolver.Object,
                mockCancellationTokenProvider.Object,
                mockCurrentTenant.Object,
                mockDbContextTypeProvider.Object,
                mockMongoClientFactory.Object)
            {
                Logger = mockLogger.Object
            };

            // Setup Uow manager to return current unit of work
            mockUowManager.Setup(m => m.Current).Returns(mockUnitOfWork.Object);
            // Setup unit of work to not disable warning
            mockUnitOfWork.Setup(m => m.EnableObsoleteDbContextCreationWarning).Returns(true);
            mockUnitOfWork.Setup(m => m.DisableObsoleteDbContextCreationWarning).Returns(false);
            // Setup unit of work to return a mock database API
            mockUnitOfWork.Setup(m => m.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IMongoDatabaseApi>>()))
                .Returns(mockDatabaseApi.Object);
            // Setup the database API to return a dummy context
            var dummyContext = new SampleMongoDbContext();
            mockDatabaseApi.Setup(m => m.DbContext).Returns(dummyContext);
            // Setup DbContextTypeProvider to return a dummy type
            mockDbContextTypeProvider.Setup(m => m.GetDbContextType(typeof(SampleMongoDbContext)))
                .Returns(typeof(SampleMongoDbContext));
            // Setup ConnectionStringResolver to return a dummy connection string
            mockConnectionStringResolver.Setup(m => m.Resolve(It.IsAny<string>())).Returns("mongodb://localhost:27017/testdb");
            // Setup MongoClientFactory to return a dummy client
            var mockMongoClient = new Mock<IMongoClient>();
            mockMongoClientFactory.Setup(m => m.Get(It.IsAny<MongoUrl>())).Returns(mockMongoClient.Object);
            // Setup MongoClient to return a database
            var mockDatabase = new Mock<IMongoDatabase>();
            mockMongoClient.Setup(m => m.GetDatabase(It.IsAny<string>())).Returns(mockDatabase.Object);

            // Act
            // Manually set the method to be virtual and call it
            var context = provider.GetDbContext();

            // Assert
            mockLogger.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy DbContext class for testing
    public class SampleMongoDbContext : IAbpMongoDbContext
    {
        public void InitializeDatabase(IMongoDatabase database, IMongoClient client, object options)
        {
            // No-op
        }
        public IAbpMongoDbContext ToAbpMongoDbContext() => this;
        // Implement other interface members as needed
        public IMongoCollection<T> Collection<T>() => throw new NotImplementedException();
        public IMongoDatabase Database => throw new NotImplementedException();
        public IMongoClient Client => throw new NotImplementedException();
    }
}
