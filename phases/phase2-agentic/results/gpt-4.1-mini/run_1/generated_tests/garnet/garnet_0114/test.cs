using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new TestableMigrateSession(loggerMock.Object);

            // Setup _namespaces to cause an exception in ExecuteForArrayAsync
            migrateSession.SetNamespaces(new ulong[] { 0, 1, 2, 3 });

            // Setup migrateOperation[0].Client.ExecuteForArrayAsync to throw
            migrateSession.SetupExecuteForArrayAsyncToThrow();

            // Act
            var result = await migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Helper class to expose internals and allow mocking dependencies
    internal class TestableMigrateSession : MigrateSession
    {
        private Mock<IMigrateOperationClient> _clientMock = new();
        private ulong[] _namespaces = Array.Empty<ulong>();

        public TestableMigrateSession(ILogger logger)
        {
            this.logger = logger;
            // Setup migrateOperation array with one element with Client property mocked
            this.migrateOperation = new IMigrateOperation[] { new TestMigrateOperation(_clientMock.Object) };
        }

        public void SetNamespaces(ulong[] namespaces)
        {
            _namespaces = namespaces;
            this._namespaces = new System.Collections.Generic.List<ulong>(_namespaces);
        }

        public void SetupExecuteForArrayAsyncToThrow()
        {
            _clientMock.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string[]>())).ThrowsAsync(new Exception("Test exception"));
        }

        // Expose protected/internal members for test
        public new System.Collections.Generic.List<ulong> _namespaces
        {
            get => base._namespaces;
            set => base._namespaces = value;
        }

        public new IMigrateOperation[] migrateOperation
        {
            get => base.migrateOperation;
            set => base.migrateOperation = value;
        }

        public new ILogger logger
        {
            get => base.logger;
            set => base.logger = value;
        }
    }

    internal interface IMigrateOperationClient
    {
        Task<string[]> ExecuteForArrayAsync(params string[] args);
    }

    internal interface IMigrateOperation
    {
        IMigrateOperationClient Client { get; }
    }

    internal class TestMigrateOperation : IMigrateOperation
    {
        public IMigrateOperationClient Client { get; }

        public TestMigrateOperation(IMigrateOperationClient client)
        {
            Client = client;
        }
    }
}
