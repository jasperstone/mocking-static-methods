using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server.Replication
{
    public class ReplicationManagerTests
    {
        [Fact]
        public void BeginRecovery_WithActiveRecoveryStatus_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var replicationManagerType = GetReplicationManagerType();
            var instance = FormatterServices.GetUninitializedObject(replicationManagerType);

            var loggerMock = new Mock<ILogger>();

            var loggerField = replicationManagerType.GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(loggerField);
            SetField(instance, loggerField!, loggerMock.Object);

            var currentRecoveryStatusField = replicationManagerType.GetField("currentRecoveryStatus", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(currentRecoveryStatusField);
            var recoveryStatusType = currentRecoveryStatusField!.FieldType;

            Assert.True(recoveryStatusType.IsEnum);
            var recoveryStatusNames = Enum.GetNames(recoveryStatusType);
            Assert.Contains("ReadRole", recoveryStatusNames);
            Assert.Contains("InitializeRecover", recoveryStatusNames);

            var currentStatus = Enum.Parse(recoveryStatusType, "ReadRole");
            currentRecoveryStatusField.SetValue(instance, currentStatus);

            var nextStatus = Enum.Parse(recoveryStatusType, "InitializeRecover");

            var beginRecoveryMethod = replicationManagerType.GetMethod("BeginRecovery", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(beginRecoveryMethod);

            // Act
            var result = beginRecoveryMethod!.Invoke(instance, new[] { nextStatus, (object)false });

            // Assert
            var boolResult = Assert.IsType<bool>(result);
            Assert.False(boolResult);

            const string expectedMessage = "Error background recovering task has not completed [InitializeRecover]";
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString() == expectedMessage),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static Type GetReplicationManagerType()
        {
            const string typeName = "Garnet.cluster.ReplicationManager";

            var existing = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(typeName, throwOnError: false))
                .FirstOrDefault(t => t != null);

            if (existing != null)
            {
                return existing;
            }

            foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                var assembly = Assembly.Load(assemblyName);
                var candidate = assembly.GetType(typeName, throwOnError: false);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            throw new InvalidOperationException($"Unable to locate type '{typeName}'.");
        }

        private static void SetField(object target, FieldInfo field, object value)
        {
            var dynamicMethod = new DynamicMethod(
                $"Set_{field.Name}",
                null,
                new[] { typeof(object), typeof(object) },
                field.DeclaringType,
                true);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, field.DeclaringType!);
            il.Emit(OpCodes.Ldarg_1);
            if (field.FieldType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, field.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, field.FieldType);
            }
            il.Emit(OpCodes.Stfld, field);
            il.Emit(OpCodes.Ret);

            var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            setter(target, value);
        }
    }
}
