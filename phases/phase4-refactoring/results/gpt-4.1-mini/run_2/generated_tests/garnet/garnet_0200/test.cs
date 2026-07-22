using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class ReplicaSyncSessionLoggingTests
    {
        [Fact]
        public async Task SendCheckpointAsync_LogsCompleteSendingCheckpointMetadata()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Use reflection to get the internal ReplicaSyncSession type
            var assembly = typeof(Garnet.cluster.ReplicaSyncSession).Assembly;
            var replicaSyncSessionType = assembly.GetType("Garnet.cluster.ReplicaSyncSession");
            Assert.NotNull(replicaSyncSessionType);

            // Prepare constructor parameters with nulls or mocks as needed
            var ctor = replicaSyncSessionType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];
            var parameters = ctor.GetParameters();

            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                if (paramType == typeof(ILogger))
                    args[i] = loggerMock.Object;
                else if (paramType == typeof(string))
                    args[i] = "replicaNodeId";
                else if (paramType.IsValueType)
                    args[i] = Activator.CreateInstance(paramType);
                else
                    args[i] = null;
            }

            // Create instance
            var instance = ctor.Invoke(args);

            // Get SendCheckpointAsync method
            var method = replicaSyncSessionType.GetMethod("SendCheckpointAsync", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(method);

            // Act
            try
            {
                var task = (Task<bool>)method.Invoke(instance, null);
                await task;
            }
            catch
            {
                // Ignored - incomplete mocks may cause exceptions
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("<Complete sending checkpoint metadata")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
