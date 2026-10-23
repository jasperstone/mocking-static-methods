using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionLoggingTests
    {
        private object CreateMigrateSessionWithMocks(out Mock<ILogger> loggerMock, out Mock<IClient> clientMock)
        {
            var migrateSessionType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateSession") 
                ?? throw new InvalidOperationException("MigrateSession type not found");

            // Create dummy parameters for constructor
            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(migrateSessionType);

            // Create mocks
            loggerMock = new Mock<ILogger>();
            clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("OK");

            // Set private readonly logger field
            var loggerField = migrateSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(instance, loggerMock.Object);

            // Set private readonly _timeout field
            var timeoutField = migrateSessionType.GetField("_timeout", BindingFlags.NonPublic | BindingFlags.Instance);
            timeoutField.SetValue(instance, TimeSpan.FromMilliseconds(100));

            // Set private readonly _cts field
            var ctsField = migrateSessionType.GetField("_cts", BindingFlags.NonPublic | BindingFlags.Instance);
            ctsField.SetValue(instance, new CancellationTokenSource());

            // Set private readonly _slotRanges field
            var slotRangesField = migrateSessionType.GetField("_slotRanges", BindingFlags.NonPublic | BindingFlags.Instance);
            slotRangesField.SetValue(instance, new object());

            // Set private readonly _sslots field
            var sslotsField = migrateSessionType.GetField("_sslots", BindingFlags.NonPublic | BindingFlags.Instance);
            sslotsField.SetValue(instance, new int[0]);

            // Set private readonly migrateOperation field to array with one element with Client property set to clientMock.Object
            var migrateOperationField = migrateSessionType.GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance);
            var migrateOperationElementType = migrateOperationField.FieldType.GetElementType();

            var migrateOperationInstance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(migrateOperationElementType);
            var clientProperty = migrateOperationElementType.GetProperty("Client", BindingFlags.Public | BindingFlags.Instance);
            clientProperty.SetValue(migrateOperationInstance, clientMock.Object);

            var array = Array.CreateInstance(migrateOperationElementType, 1);
            array.SetValue(migrateOperationInstance, 0);
            migrateOperationField.SetValue(instance, array);

            return instance;
        }

        private Task<bool> CallTrySetSlotRangesAsync(object migrateSessionInstance, string nodeid, object state)
        {
            var method = migrateSessionInstance.GetType().GetMethod("TrySetSlotRangesAsync", BindingFlags.Public | BindingFlags.Instance);
            return (Task<bool>)method.Invoke(migrateSessionInstance, new object[] { nodeid, state })!;
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenResultNotOk()
        {
            var migrateSessionType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateSession")!;
            var migrateStateType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateState")!;
            var stableState = Enum.Parse(migrateStateType, "STABLE");

            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("FAIL");

            var instance = CreateMigrateSessionWithMocks(out var lm, out _);
            // Replace clientMock in migrateOperation
            var migrateOperationField = migrateSessionType.GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance);
            var migrateOperationArray = (Array)migrateOperationField.GetValue(instance);
            var migrateOperationElement = migrateOperationArray.GetValue(0);
            var clientProperty = migrateOperationElement.GetType().GetProperty("Client");
            clientProperty.SetValue(migrateOperationElement, clientMock.Object);

            // Replace logger mock with our own to verify
            var loggerField = migrateSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(instance, loggerMock.Object);

            var result = await CallTrySetSlotRangesAsync(instance, "node1", stableState);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_OnOperationCanceledException()
        {
            var migrateSessionType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateSession")!;
            var migrateStateType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateState")!;
            var stableState = Enum.Parse(migrateStateType, "STABLE");

            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(() => Task.FromException<string>(new OperationCanceledException()));

            var instance = CreateMigrateSessionWithMocks(out var lm, out _);
            var migrateOperationField = migrateSessionType.GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance);
            var migrateOperationArray = (Array)migrateOperationField.GetValue(instance);
            var migrateOperationElement = migrateOperationArray.GetValue(0);
            var clientProperty = migrateOperationElement.GetType().GetProperty("Client");
            clientProperty.SetValue(migrateOperationElement, clientMock.Object);

            var loggerField = migrateSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(instance, loggerMock.Object);

            var result = await CallTrySetSlotRangesAsync(instance, "node1", stableState);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SetSlotRange operation timed out or was cancelled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_OnGeneralException()
        {
            var migrateSessionType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateSession")!;
            var migrateStateType = typeof(object).Assembly.GetType("Garnet.cluster.MigrateState")!;
            var stableState = Enum.Parse(migrateStateType, "STABLE");

            var loggerMock = new Mock<ILogger>();
            var clientMock = new Mock<IClient>();
            clientMock.Setup(c => c.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<object>()))
                .Returns(() => Task.FromException<string>(new Exception("fail")));

            var instance = CreateMigrateSessionWithMocks(out var lm, out _);
            var migrateOperationField = migrateSessionType.GetField("migrateOperation", BindingFlags.NonPublic | BindingFlags.Instance);
            var migrateOperationArray = (Array)migrateOperationField.GetValue(instance);
            var migrateOperationElement = migrateOperationArray.GetValue(0);
            var clientProperty = migrateOperationElement.GetType().GetProperty("Client");
            clientProperty.SetValue(migrateOperationElement, clientMock.Object);

            var loggerField = migrateSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(instance, loggerMock.Object);

            var result = await CallTrySetSlotRangesAsync(instance, "node1", stableState);

            Assert.False(result);
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred during SetSlotRange")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    // Dummy interface to match client type
    internal interface IClient
    {
        Task<string> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges);
    }
}
