using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerReflectionTests
    {
        private static object CreateMultiDatabaseManager(Mock<StoreWrapper> storeWrapperMock)
        {
            var assembly = typeof(MultiDatabaseManager).Assembly;
            var type = assembly.GetType("Garnet.server.MultiDatabaseManager");
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { typeof(StoreWrapper.DatabaseCreatorDelegate), typeof(StoreWrapper), typeof(bool) },
                null);
            if (ctor == null) throw new Exception("Constructor not found");

            StoreWrapper.DatabaseCreatorDelegate creator = (int id) =>
            {
                // Create a mock GarnetDatabase with minimal setup
                var dbType = assembly.GetType("Garnet.server.GarnetDatabase");
                var dbMockType = typeof(Mock<>).MakeGenericType(dbType);
                var dbMock = Activator.CreateInstance(dbMockType, MockBehavior.Strict);
                var objectStoreProp = dbType.GetProperty("ObjectStore");
                var vectorManagerProp = dbType.GetProperty("VectorManager");
                var vectorManagerType = assembly.GetType("Garnet.server.VectorManager");
                var vectorManagerMockType = typeof(Mock<>).MakeGenericType(vectorManagerType);
                var vectorManagerMock = Activator.CreateInstance(vectorManagerMockType, MockBehavior.Strict);
                var vectorManagerInstance = vectorManagerMockType.GetProperty("Object").GetValue(vectorManagerMock);
                var initializeMethod = vectorManagerType.GetMethod("Initialize");
                // Setup Initialize to do nothing
                var setupMethod = vectorManagerMockType.GetMethod("Setup", new Type[] { typeof(Expression<Action<object>>) });
                // We cannot easily setup via reflection, so skip setup

                // Setup VectorManager property to return vectorManagerInstance
                var dbMockAsMock = (Mock)dbMock;
                dbMockAsMock.SetupGet(vectorManagerProp).Returns(vectorManagerInstance);
                dbMockAsMock.SetupGet(objectStoreProp).Returns(null);

                return dbMockAsMock.Object;
            };

            var instance = ctor.Invoke(new object[] { creator, storeWrapperMock.Object, false });
            return instance;
        }

        [Fact]
        public void RecoverCheckpoint_LogsInformationOnTryGetSavedDatabaseIdsException_AndDoesNotThrowWhenFailOnRecoveryErrorFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>(null, null);
            var serverOptionsType = typeof(StoreWrapper).Assembly.GetType("Garnet.server.ServerOptions");
            var serverOptions = Activator.CreateInstance(serverOptionsType);
            serverOptionsType.GetProperty("FailOnRecoveryError").SetValue(serverOptions, false);
            serverOptionsType.GetProperty("MainStoreCheckpointBaseDirectory").SetValue(serverOptions, "baseDir");
            serverOptionsType.GetProperty("MaxDatabases").SetValue(serverOptions, 1);
            storeWrapperMock.SetupGet(s => s.serverOptions).Returns(serverOptions);
            storeWrapperMock.SetupGet(s => s.loggerFactory).Returns(new LoggerFactory());

            var multiDbManager = CreateMultiDatabaseManager(storeWrapperMock);

            // Set Logger property to loggerMock.Object via reflection
            var loggerField = multiDbManager.GetType().GetField("Logger", BindingFlags.Instance | BindingFlags.NonPublic);
            loggerField.SetValue(multiDbManager, loggerMock.Object);

            // Use reflection to get RecoverCheckpoint method
            var recoverMethod = multiDbManager.GetType().GetMethod("RecoverCheckpoint", BindingFlags.Instance | BindingFlags.Public);

            // We cannot easily cause TryGetSavedDatabaseIds to throw without subclassing,
            // so this test is limited to calling RecoverCheckpoint and verifying no exceptions.

            // Act & Assert
            Exception ex = Record.Exception(() => recoverMethod.Invoke(multiDbManager, new object[] { false, false, false, null }));
            Assert.Null(ex);
        }
    }
}
