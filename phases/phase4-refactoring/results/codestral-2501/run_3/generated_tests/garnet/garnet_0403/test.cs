using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System;
using System.Threading.Tasks;

public class MultiDatabaseManagerTests
{
    [Fact]
    public void LogInformation_Called_When_RecoverDatabaseCheckpoint_Throws()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();
        storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
        storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new GarnetServerOptions { FailOnRecoveryError = false });

        var multiDatabaseManager = new MultiDatabaseManager(Mock.Of<StoreWrapper.DatabaseCreatorDelegate>(), storeWrapperMock.Object);
        multiDatabaseManager.Logger = loggerMock.Object;

        var dbMock = new Mock<GarnetDatabase>();
        dbMock.Setup(db => db.MainStore).Returns(Mock.Of<TsavoriteKV<SpanByte, SpanByte, MainStoreFunctions, MainStoreAllocator>>());
        dbMock.Setup(db => db.ObjectStore).Returns(Mock.Of<TsavoriteKV<byte[], IGarnetObject, ObjectStoreFunctions, ObjectStoreAllocator>>());

        multiDatabaseManager.TryGetOrAddDatabase(0, out var success, out var added);
        multiDatabaseManager.databases.Map[0] = dbMock.Object;

        // Act
        multiDatabaseManager.RecoverCheckpoint();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
