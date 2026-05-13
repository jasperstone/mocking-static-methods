using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Databases
{
    public class MultiDatabaseManagerTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly TestMultiDatabaseManager _manager;
        private readonly Mock<IGarnetDatabase> _dbMock;
        private readonly StoreOptions _storeOptions;

        public MultiDatabaseManagerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _storeOptions = new StoreOptions { FailOnRecoveryError = false };
            _dbMock = new Mock<IGarnetDatabase>();

            var storeWrapper = new StoreWrapper(
                id => _dbMock.Object,
                _storeOptions,
                new Mock<ILoggerFactory>().Object);

            _manager = new TestMultiDatabaseManager(storeWrapper);
            _manager.SetLogger(_loggerMock.Object);
        }

        [Fact]
        public void RecoverCheckpoint_GeneralExceptionDuringRecovery_LogsInformation()
        {
            var exception = new InvalidOperationException("test exception");
            _manager.RecoverDatabaseCheckpointBehavior = () => throw exception;

            _manager.RecoverCheckpoint();

            _loggerMock.VerifyLogInformation(
                exception,
                "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
                0L,
                0L);
        }

        private class TestMultiDatabaseManager : MultiDatabaseManager
        {
            private readonly List<int> _dbIds;

            public Action RecoverDatabaseCheckpointBehavior { get; set; } = () => { };

            public TestMultiDatabaseManager(StoreWrapper storeWrapper)
                : base(_ => new TestGarnetDatabase(), storeWrapper, createDefaultDatabase: false)
            {
                _dbIds = new List<int> { 0 };
                SetActiveDatabaseIds(_dbIds.ToArray());
            }

            protected override void RecoverDatabaseCheckpoint(GarnetDatabase database, out long storeVersion, out long objectStoreVersion)
            {
                storeVersion = 0;
                objectStoreVersion = 0;
                RecoverDatabaseCheckpointBehavior();
            }

            public void SetLogger(ILogger logger)
            {
                Logger = logger;
            }

            private void SetActiveDatabaseIds(int[] dbIds)
            {
                typeof(MultiDatabaseManager).GetField("activeDbIds", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(this, new ExpandableMap<int>(dbIds.Length, 0, dbIds.Length - 1, dbIds));
            }
        }

        private class StoreWrapper
        {
            public StoreWrapper(Func<int, IGarnetDatabase> createDatabaseDelegate, StoreOptions serverOptions, ILoggerFactory loggerFactory)
            {
                CreateDatabaseDelegate = createDatabaseDelegate;
                ServerOptions = serverOptions;
                LoggerFactory = loggerFactory;
            }

            public Func<int, IGarnetDatabase> CreateDatabaseDelegate { get; }
            public StoreOptions ServerOptions { get; }
            public ILoggerFactory LoggerFactory { get; }
        }

        private class StoreOptions
        {
            public bool FailOnRecoveryError { get; set; }
        }

        private class TestGarnetDatabase : IGarnetDatabase
        {
            public IVectorManager VectorManager { get; } = new Mock<IVectorManager>().Object;
            public object ObjectStore => null;
        }

        private interface IGarnetDatabase
        {
            IVectorManager VectorManager { get; }
            object ObjectStore { get; }
        }

        private interface IVectorManager
        {
            void Initialize();
        }
    }

    internal static class LoggerExtensions
    {
        public static void VerifyLogInformation(this Mock<ILogger> loggerMock, Exception exception, string format, params object[] args)
        {
            loggerMock.Verify(logger =>
                logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => CheckLogMessage(format, args, v)),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>())
            );
        }

        private static bool CheckLogMessage(string format, object[] args, object state)
        {
            var formatted = string.Format(format, args.Select((arg, idx) => $"{{{idx}}}").ToArray());
            return state.ToString() == string.Format(formatted, args);
        }
    }
}
