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

namespace Volo.Abp.Uow.MongoDB.Tests;

public class UnitOfWorkMongoDbContextProviderTests
{
    private const string TransactionsNotSupportedWarningMessage = "Current database does not support transactions. Your database may remain in an inconsistent state in an error case.";

    [Fact]
    public async Task CreateDbContextWithTransactionAsync_Should_LogWarning_When_TransactionNotSupported()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>>>();
        loggerMock.Setup(l => l.LogWarning(TransactionsNotSupportedWarningMessage));

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<IMyMongoDbContext>()).Returns(Mock.Of<IMyMongoDbContext>());
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((MongoDbTransactionApi)null);

        var mongoClientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();
        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());

        var mongoUrl = new MongoUrl("mongodb://localhost/testdb");
        var databaseMock = new Mock<IMongoDatabase>();

        var provider = new TestableUnitOfWorkMongoDbContextProvider<IMyMongoDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<ICancellationTokenProvider>(),
            Mock.Of<ICurrentTenant>(),
            Mock.Of<IMongoDbContextTypeProvider>(),
            Mock.Of<IAbpMongoClientFactory>().Object,
            loggerMock.Object
        )
        {
            Client_StartSessionAsync = ct => Task.FromResult(sessionMock.Object)
        };

        // Act
        var dbContext = await provider.CreateDbContextWithTransactionAsync(
            unitOfWorkMock.Object,
            mongoUrl,
            mongoClientMock.Object,
            databaseMock.Object
        );

        // Assert
        loggerMock.Verify(l => l.LogWarning(TransactionsNotSupportedWarningMessage), Times.Once);
        Assert.NotNull(dbContext);
    }

    public class TestableUnitOfWorkMongoDbContextProvider<TMongoDbContext> : UnitOfWorkMongoDbContextProvider<TMongoDbContext>
        where TMongoDbContext : IAbpMongoDbContext
    {
        public Func<CancellationToken, Task<IClientSessionHandle>> Client_StartSessionAsync;

        public TestableUnitOfWorkMongoDbContextProvider(
            IUnitOfWorkManager unitOfWorkManager,
            IConnectionStringResolver connectionStringResolver,
            ICancellationTokenProvider cancellationTokenProvider,
            ICurrentTenant currentTenant,
            IMongoDbContextTypeProvider dbContextTypeProvider,
            IAbpMongoClientFactory mongoClientFactory,
            ILogger<UnitOfWorkMongoDbContextProvider<TMongoDbContext>> logger)
            : base(unitOfWorkManager, connectionStringResolver, cancellationTokenProvider, currentTenant, dbContextTypeProvider, mongoClientFactory)
        {
            Logger = logger;
        }

        protected override async Task<TMongoDbContext> CreateDbContextWithTransactionAsync(
            IUnitOfWork unitOfWork,
            MongoUrl url,
            MongoClient client,
            IMongoDatabase database,
            CancellationToken cancellationToken = default)
        {
            var transactionApiKey = $"MongoDb_{url}";
            var activeTransaction = unitOfWork.FindTransactionApi(transactionApiKey) as MongoDbTransactionApi;
            var dbContext = unitOfWork.ServiceProvider.GetRequiredService<TMongoDbContext>();

            if (activeTransaction?.SessionHandle == null)
            {
                var session = await Client_StartSessionAsync(GetCancellationToken(cancellationToken)) ?? await client.StartSessionAsync(cancellationToken: GetCancellationToken(cancellationToken));

                if (unitOfWork.Options.Timeout.HasValue)
                {
                    session.AdvanceOperationTime(new BsonTimestamp(unitOfWork.Options.Timeout.Value));
                }

                try
                {
                    session.StartTransaction();
                }
                catch (NotSupportedException)
                {
                    Logger.LogWarning(TransactionsNotSupportedWarningMessage);

                    dbContext.ToAbpMongoDbContext().InitializeDatabase(database, client, null);
                    return dbContext;
                }

                unitOfWork.AddTransactionApi(
                    transactionApiKey,
                    new MongoDbTransactionApi(
                        session,
                        CancellationTokenProvider
                    )
                );

                dbContext.ToAbpMongoDbContext().InitializeDatabase(database, client, session);
            }
            else
            {
                dbContext.ToAbpMongoDbContext().InitializeDatabase(database, client, activeTransaction.SessionHandle);
            }

            return dbContext;
        }
    }

    public interface IMyMongoDbContext : IAbpMongoDbContext { }
}
