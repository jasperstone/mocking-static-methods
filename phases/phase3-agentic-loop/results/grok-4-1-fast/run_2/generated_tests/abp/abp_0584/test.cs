using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.Uow;
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
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(TransactionsNotSupportedWarningMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            ).Verifiable();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMyMongoDbContext>(Mock.Of<IMyMongoDbContext>())
            .BuildServiceProvider();
        unitOfWorkMock.Setup(u => u.ServiceProvider).Returns(serviceProvider);
        var optionsMock = new Mock<IUnitOfWorkOptions>();
        optionsMock.Setup(o => o.IsTransactional).Returns(true);
        unitOfWorkMock.Setup(u => u.Options).Returns(optionsMock.Object);
        unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns((ITransactionApi?)null);

        var clientMock = new Mock<MongoClient>();
        var sessionMock = new Mock<IClientSessionHandle>();
        clientMock.Setup(c => c.StartSessionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionMock.Object);
        sessionMock.Setup(s => s.StartTransaction()).Throws(new NotSupportedException());

        var provider = new UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>(
            Mock.Of<IUnitOfWorkManager>(),
            Mock.Of<IConnectionStringResolver>(),
            Mock.Of<Volo.Abp.Threading.ICancellationTokenProvider>(),
            Mock.Of<Volo.Abp.MultiTenancy.ICurrentTenant>(),
            Mock.Of<Volo.Abp.MongoDB.IMongoDbContextTypeProvider>(),
            Mock.Of<Volo.Abp.MongoDB.Clients.IAbpMongoClientFactory>()
        )
        {
            Logger = loggerMock.Object
        };

        // Use reflection to call the protected method
        var method = typeof(UnitOfWorkMongoDbContextProvider<IMyMongoDbContext>)
            .GetMethod("CreateDbContextWithTransactionAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
        var mongoUrl = new MongoUrl("mongodb://localhost");
        var database = Mock.Of<IMongoDatabase>();

        // Act
        await (Task<IMyMongoDbContext>)method.Invoke(provider, new object[] { unitOfWorkMock.Object, mongoUrl, clientMock.Object, database, CancellationToken.None })!;

        // Assert
        loggerMock.Verify();
    }
}

public interface IMyMongoDbContext : IAbpMongoDbContext
{
}
