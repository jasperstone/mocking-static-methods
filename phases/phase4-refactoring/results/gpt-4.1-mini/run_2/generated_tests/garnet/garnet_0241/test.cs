using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicationManagerTests
    {
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found on type '{obj.GetType().FullName}'");
            field.SetValue(obj, value);
        }

        [Fact]
        public unsafe void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var type = Type.GetType("Garnet.cluster.ReplicationManager, Garnet");
            Assert.NotNull(type);
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);
            Assert.NotNull(ctor);
            var manager = ctor.Invoke(null);

            var clusterProviderField = type.GetField("clusterProvider", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(clusterProviderField);
            var clusterProvider = clusterProviderField.GetValue(manager);

            var replicationManagerField = clusterProvider.GetType().GetField("replicationManager", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(replicationManagerField);
            var replicationManager = replicationManagerField.GetValue(clusterProvider);

            var cannotStreamAofField = replicationManager.GetType().GetField("CannotStreamAOF", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotNull(cannotStreamAofField);
            cannotStreamAofField.SetValue(replicationManager, true);

            var loggerMock = new Mock<ILogger>();
            SetPrivateField(manager, "logger", loggerMock.Object);

            // Prepare dummy record data
            byte[] recordBytes = new byte[1] { 0x00 };

            fixed (byte* p = recordBytes)
            {
                var method = type.GetMethod("ProcessPrimaryStream", BindingFlags.Instance | BindingFlags.Public);
                // Act & Assert
                var ex = Assert.Throws<TargetInvocationException>(() =>
                {
                    method.Invoke(manager, new object[] { (IntPtr)p, 1, 0L, 0L, 0L });
                });
                // The actual exception is inner exception
                Assert.IsType<GarnetException>(ex.InnerException);
                Assert.Contains("Replica is recovering cannot sync AOF", ex.InnerException.Message);
            }

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica is recovering cannot sync AOF")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
