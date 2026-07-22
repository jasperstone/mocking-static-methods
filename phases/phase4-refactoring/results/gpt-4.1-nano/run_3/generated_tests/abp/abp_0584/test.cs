using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Uow.MongoDB;
using MongoDB.Driver;

namespace Volo.Abp.MongoDB.Tests
{
    public class UnitOfWorkMongoDbContextProviderTests
    {
        [Fact]
        public async Task CreateDbContextWithTransaction_ShouldLogWarning_WhenStartTransactionThrowsNotSupportedException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UnitOfWorkMongoDbContextProvider<MockDbContext>>>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var unitOfWorkManagerMock = new Mock<IUnitOfWorkManager>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var mongoClientMock = new Mock<IMongoClient>();
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            var sessionMock = new Mock<IClientSessionHandle>();
            var optionsMock = new Mock<IUnitOfWorkOptions>();
            var transactionApiMock = new Mock<MongoDbTransactionApi>();
            var provider = new UnitOfWorkMongoDbContextProvider<MockDbContext>(
                unitOfWorkManagerMock.Object,
                Mock.Of<IConnectionStringResolver>(),
                Mock.Of<ICancellationTokenProvider>(),
                Mock.Of<ICurrentTenant>(),
                Mock.Of<IMongoDbContextTypeProvider>(),
                Mock.Of<IAbpMongoClientFactory>()
            );

            provider.Logger = loggerMock.Object;

            // Setup unitOfWork
            unitOfWorkMock.SetupGet(u => u.Options).Returns(optionsMock.Object);
            optionsMock.SetupGet(o => o.IsTransactional).Returns(true);
            unitOfWorkMock.SetupGet(u => u.ServiceProvider).Returns(serviceProviderMock.Object);
            unitOfWorkMock.Setup(u => u.FindTransactionApi(It.IsAny<string>())).Returns(transactionApiMock.Object);
            unitOfWorkMock.Setup(u => u.Options.Timeout).Returns(TimeSpan.FromSeconds(30));
            unitOfWorkMock.Setup(u => u.ServiceProvider.GetRequiredService<MockDbContext>())
                .Returns(new MockDbContext());

            // Setup session to throw NotSupportedException
            sessionMock.Setup(s => s.StartTransaction()).Throws<NotSupportedException>();
            // Setup client to return session
            mongoClientMock.Setup(c => c.StartSession()).Returns(sessionMock.Object);
            // Setup MongoClientFactory to return our mock client
            var factoryMock = new Mock<IAbpMongoClientFactory>();
            factoryMock.Setup(f => f.Get(It.IsAny<MongoUrl>())).Returns(mongoClientMock.Object);
            provider.MongoClientFactory = factoryMock.Object;

            // Act
            var result = await provider.CreateDbContextWithTransaction(
                unitOfWorkMock.Object,
                new MongoUrl("mongodb://localhost:27017"),
                mongoClientMock.Object,
                mongoDatabaseMock.Object
            );

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Current database does not support transactions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Dummy DbContext for testing
        public class MockDbContext : IAbpMongoDbContext
        {
            public void InitializeDatabase(IMongoDatabase database, IMongoClient client, IClientSessionHandle session) { }
            public IAbpMongoDbContext ToAbpMongoDbContext() => this;
        }
    }
}
