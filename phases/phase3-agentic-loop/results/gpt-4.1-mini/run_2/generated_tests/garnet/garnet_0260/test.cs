using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster
{
    internal class ReplicationManagerTests
    {
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(obj, value);
        }

        private static object GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(obj);
        }

        private static object InvokeBeginRecovery(object rmInstance, object nextRecoveryStatus, bool upgradeLock)
        {
            var method = rmInstance.GetType().GetMethod("BeginRecovery", BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(rmInstance, new object[] { nextRecoveryStatus, upgradeLock });
        }

        [Fact]
        public void BeginRecovery_LogsError_WhenCurrentRecoveryStatusNotNoRecovery()
        {
            var loggerMock = new Mock<ILogger>();
            var rmType = typeof(object).Assembly.GetType("Garnet.cluster.ReplicationManager");
            Assert.NotNull(rmType);

            // Create instance with null clusterProvider and loggerMock.Object
            var rmInstance = Activator.CreateInstance(rmType, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new object[] { null, loggerMock.Object }, null);
            Assert.NotNull(rmInstance);

            // Set currentRecoveryStatus to something other than NoRecovery (assume enum int 1)
            var recoveryStatusType = rmType.Assembly.GetType("Garnet.cluster.RecoveryStatus");
            Assert.NotNull(recoveryStatusType);
            var initializeRecoverValue = Enum.Parse(recoveryStatusType, "InitializeRecover");

            SetPrivateField(rmInstance, "currentRecoveryStatus", initializeRecoverValue);

            var readRoleValue = Enum.Parse(recoveryStatusType, "ReadRole");

            // Act
            var result = (bool)InvokeBeginRecovery(rmInstance, readRoleValue, false);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error background recovering task has not completed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
