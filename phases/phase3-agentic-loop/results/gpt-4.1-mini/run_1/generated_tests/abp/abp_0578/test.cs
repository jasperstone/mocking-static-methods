using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        private class TestMongoDbContext : IAbpMongoDbContext
        {
            public IMongoClient Client => null!;
            public IMongoDatabase Database => null!;
            public IClientSessionHandle? SessionHandle => null;

            public IMongoCollection<T> Collection<T>()
            {
                return null!;
            }
        }

        private class DummyUnitOfWorkOptions : AbpUnitOfWorkOptions
        {
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            // Setup UnitOfWorkManager.Current to return a mock unit of work
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Options).Returns(new DummyUnitOfWorkOptions());
            unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
                .Returns((string key, Func<IDatabaseApi> factory) => factory());

            unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup static flags for obsolete warning
            typeof(UnitOfWork).GetField("EnableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, true);
            typeof(Uow.UnitOfWorkManager).GetField("DisableObsoleteDbContextCreationWarning", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                ?.SetValue(null, new Lazy<bool>(() => false));

            // Setup DbContextTypeProvider to return the type of TestMongoDbContext
            dbContextTypeProviderMock.Setup(d => d.GetDbContextType(typeof(TestMongoDbContext)))
                .Returns(typeof(TestMongoDbContext));

            // Setup ConnectionStringResolver to return a valid connection string
            connectionStringResolverMock.Setup(c => c.ResolveAsync(It.IsAny<Type>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("mongodb://localhost:27017/testdb");

            connectionStringResolverMock.Setup(c => c.Resolve(It.IsAny<Type>()))
                .Returns("mongodb://localhost:27017/testdb");

            // Setup MongoClientFactory to return a mock MongoClient
            var mongoClientMock = new Mock<MongoClient>(new MongoClientSettings());
            mongoClientFactoryMock.Setup(f => f.Get(It.IsAny<MongoUrl>())).Returns(mongoClientMock.Object);

            // Setup MongoClient.GetDatabase to return a mock IMongoDatabase
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoClientMock.Setup(c => c.GetDatabase(It.IsAny<string>(), null)).Returns(mongoDatabaseMock.Object);

            var provider = new UnitOfWorkMongoDbContextProvider<TestMongoDbContext>(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            // Act
            // We expect the call to GetDbContext to log warnings
            try
            {
                provider.GetDbContext();
            }
            catch (AbpException)
            {
                // Expected because the IServiceProvider is not fully mocked to provide TestMongoDbContext
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("at ")), // StackTrace contains "at "
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
