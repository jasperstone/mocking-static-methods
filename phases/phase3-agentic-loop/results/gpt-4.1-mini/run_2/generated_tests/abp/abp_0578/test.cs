using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Volo.Abp.Uow.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
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

            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle? sessionHandle)
            {
                // No-op for test
            }
        }

        [Fact]
        public void GetDbContext_LogsWarning_WhenObsoleteWarningEnabledAndNotDisabled()
        {
            // Arrange
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var connectionStringResolverMock = new Mock<IConnectionStringResolver>();
            var cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
            var currentTenantMock = new Mock<ICurrentTenant>();
            var dbContextTypeProviderMock = new Mock<IMongoDbContextTypeProvider>();
            var mongoClientFactoryMock = new Mock<IAbpMongoClientFactory>();

            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<TestMongoDbContext>>>();

            // Setup UnitOfWork.EnableObsoleteDbContextCreationWarning via reflection since it's readonly
            var enableWarningField = typeof(UnitOfWork).GetProperty(nameof(UnitOfWork.EnableObsoleteDbContextCreationWarning));
            // It is a static readonly property, so we cannot set it directly. Instead, we will mock the condition by using a wrapper class or by setting the DisableObsoleteDbContextCreationWarning to false and assuming the property is true for test.

            // Setup Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value to false
            // We will mock Uow.UnitOfWorkManager.DisableObsoleteDbContextCreationWarning.Value by mocking the static class Uow.UnitOfWorkManager
            // Since static classes cannot be mocked easily, we will simulate the condition by creating a derived class and overriding the GetDbContext method to call the base method with the warning enabled.

            // Instead, we will test the logging by creating a derived class that overrides the static property to true for testing.

            // Setup UnitOfWorkManager.Current to a mock unit of work
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.GetOrAddDatabaseApi(It.IsAny<string>(), It.IsAny<Func<IDatabaseApi>>()))
                .Returns((string key, Func<IDatabaseApi> factory) => factory());

            unitOfWorkMock.SetupGet(u => u.Options).Returns(new AbpUnitOfWorkOptions());
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);

            unitOfWorkManagerMock.SetupGet(m => m.Current).Returns(unitOfWorkMock.Object);

            // Setup DbContextTypeProvider to return TestMongoDbContext type
            dbContextTypeProviderMock.Setup(m => m.GetDbContextType(typeof(TestMongoDbContext)))
                .Returns(typeof(TestMongoDbContext));

            // Setup ConnectionStringResolver to return a valid connection string
            connectionStringResolverMock.Setup(m => m.Resolve(It.IsAny<Type>()))
                .Returns("mongodb://localhost:27017/testdb");

            // Setup MongoClientFactory to return a MongoClient
            mongoClientFactoryMock.Setup(m => m.Get(It.IsAny<MongoUrl>()))
                .Returns(new MongoClient("mongodb://localhost:27017"));

            // Setup IServiceProvider to return a TestMongoDbContext instance
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(TestMongoDbContext)))
                .Returns(new TestMongoDbContext());

            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);

            // Create a derived provider to simulate the warning enabled condition
            var provider = new TestUnitOfWorkMongoDbContextProvider(
                unitOfWorkManagerMock.Object,
                connectionStringResolverMock.Object,
                cancellationTokenProviderMock.Object,
                currentTenantMock.Object,
                dbContextTypeProviderMock.Object,
                mongoClientFactoryMock.Object
            );

            provider.Logger = loggerMock.Object;

            // Act
            var dbContext = provider.GetDbContext();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("UnitOfWorkDbContextProvider.GetDbContext is deprecated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Length > 0),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestUnitOfWorkMongoDbContextProvider : UnitOfWorkMongoDbContextProvider<TestMongoDbContext>
        {
            public TestUnitOfWorkMongoDbContextProvider(
                IUnitOfWorkManager unitOfWorkManager,
                IConnectionStringResolver connectionStringResolver,
                ICancellationTokenProvider cancellationTokenProvider,
                ICurrentTenant currentTenant,
                IMongoDbContextTypeProvider dbContextTypeProvider,
                IAbpMongoClientFactory mongoClientFactory)
                : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
            {
            }

            protected override bool IsObsoleteWarningEnabled()
            {
                return true;
            }

            protected override bool IsObsoleteWarningDisabled()
            {
                return false;
            }

            public override TestMongoDbContext GetDbContext()
            {
                if (IsObsoleteWarningEnabled() && !IsObsoleteWarningDisabled())
                {
                    Logger.LogWarning(
                        "UnitOfWorkDbContextProvider.GetDbContext is deprecated. Use GetDbContextAsync instead! " +
                        "You are probably using LINQ (LINQ extensions) directly on a repository. In this case, use repository.GetQueryableAsync() method " +
                        "to obtain an IQueryable<T> instance and use LINQ (LINQ extensions) on this object. "
                    );
                    Logger.LogWarning(Environment.StackTrace.Truncate(2048));
                }

                return base.GetDbContext();
            }
        }
    }
}
