using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server.Failover
{
    public class ReplicaFailoverSessionTests
    {
        private static object CreateFailoverSession(
            object clusterProvider,
            object oldConfig,
            object logger,
            object epoch = null,
            TimeSpan? failoverTimeout = null)
        {
            var type = Type.GetType("Garnet.cluster.FailoverSession, cluster", throwOnError: true);
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                new[]
                {
                    clusterProvider.GetType(),
                    oldConfig.GetType(),
                    typeof(Guid),
                    typeof(TimeSpan),
                    typeof(ILogger)
                },
                modifiers: null);

            return ctor.Invoke(new[]
            {
                clusterProvider,
                oldConfig,
                epoch ?? Guid.NewGuid(),
                (object)(failoverTimeout ?? TimeSpan.FromSeconds(30)),
                logger
            });
        }

        [Fact]
        public async Task BroadcastConfigAndRequestAttachAsync_LogsCriticalOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
            loggerMock
                .Setup(l => l.Log(
                    LogLevel.Critical,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var clusterProviderStub = new
            {
                clusterManager = new
                {
                    CurrentConfig = new MockFailedConfig()
                }
            };

            var oldConfigStub = new MockOldConfig();

            var session = CreateFailoverSession(
                clusterProviderStub,
                oldConfigStub,
                loggerMock.Object);

            var method = session.GetType().GetMethod(
                "BroadcastConfigAndRequestAttachAsync",
                BindingFlags.Instance | BindingFlags.NonPublic);

            var replicaId = "replica";
            var configBytes = new byte[] { 1, 2, 3 };

            // Act
            await (Task)method.Invoke(session, new object[] { replicaId, configBytes });

            // Assert
            loggerMock.VerifyAll();
        }

        private sealed class MockFailedConfig
        {
            public object LocalNodeId => throw new InvalidOperationException("Failure to trigger LogCritical");
        }

        private sealed class MockOldConfig
        {
            public string LocalNodePrimaryId => "primary";

            public object GetEndpointFromNodeId(string nodeId)
            {
                return new { nodeId };
            }
        }
    }
}
