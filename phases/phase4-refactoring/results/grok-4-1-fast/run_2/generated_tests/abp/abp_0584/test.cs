using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MongoDB.Clients;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Uow.MongoDB;
using Xunit;

namespace Volo.Abp.Uow.MongoDB;

public class UnitOfWorkMongoDbContextProviderTests
{
    private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

    [Fact]
    public async Task CreateDbContextWithTransactionAsync_Should_LogWarning_When_TransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
        unitOfWorkManagerMock.Setup(m => m.Current).Returns(unitOfWorkMock.Object);
        
        var dbContextMock = new Mock<IMyMongoDbContext>();
        var abpDbContextMock = new Mock<AbpMongoDbContext>();
        dbContextMock.Setup(x => x.ToAbpMongoDbContext()).Returns(abpDbContextMock.Object);
        
        var serviceProviderMock = new Mock<IServiceProvider>();
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
        serviceProviderMock.Setup(s => s.GetRequiredService<IMyMongoDbContext>())
            .Returns(dbContextMock.Object);
        
        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();
        mongoClientMock.Setup(c => c.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionMock.Object);
        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());
        
        var mongoUrl = new MongoUrl("mongodb://localhost");
        var databaseMock = new Mock<IMongoDatabase>();
        
        var unitOfWorkOptionsMock = new Mock<IUnitOfWorkOptions>();
        unitOfWorkOptionsMock.Setup(o => o.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.Options).Returns(unitOfWorkOptionsMock.Object);

        var provider = new TestableUnitOfWorkMongoDbContextProvider<IMyMongoDbContext>(
            unitOfWorkManagerMock.Object,
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IMongoDbContextTypeProvider>(),
            Mock.Of<IAbpMongoClientFactory>())
        {
            Logger = loggerMock.Object
        };

        // Act
        await provider.CallCreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            mongoUrl,
            mongoClientMock.Object,
            databaseMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogWarning(TransactionsNotSupportedWarningMessage), Times.Once);
    }

    public class TestableUnitOfWorkMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
        where TMongoDbContext : IAbpMongoDbContext
    {
        public TestableUnitOfWorkMongoDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver,
            ICancellationTokenProvider cancellationTokenProvider,
            ICurrentTenant currentTenant,
            IMongoDbContextTypeProvider dbContextTypeProvider,
            IAbpMongoClientFactory mongoClientFactory)
            : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
        {
        }

        public virtual async Task<TMongoDbContext> CallCreateDbContextWithTransactionAsync(
            IUnitOfWork unitOfWork,
            MongoUrl url,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default)
        {
            return await CreateDbContextWithTransactionAsync(unitOfWork, url, client, database, cancellationToken);
        }
    }
}

public interface IMyMongoDbContext : IAbpMongoDbContext
{
}
