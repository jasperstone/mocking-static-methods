using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Garnet.cluster.Tests
{
    public class AofTaskStoreTests
    {
        [Fact]
        public void TryAddReplicationTasks_LogsError_WhenStartAddressLessThanTruncatedUntilAndAllowDataLossFalse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Protected()
                .Setup("Log",
                    LogLevel.Error,
                    ItExpr.IsAny<EventId>(),
                    ItExpr.IsAny<It.IsAnyType>(),
                    ItExpr.IsAny<Exception>(),
                    ItExpr.IsAny<Func<It.IsAnyType, Exception, string>>())
                .Verifiable();

            var mockClusterProvider = new Mock<object>(); // Mock concrete type used by AofTaskStore constructor
            mockClusterProvider.SetupProperty(p => ((dynamic)p).AllowDataLoss, false);

            // Create AofTaskStore using reflection since it's internal
            var storeType = Type.GetType("Garnet.cluster.AofTaskStore, Garnet.cluster");
            var store = Activator.CreateInstance(storeType!, mockClusterProvider.Object, 1, mockLogger.Object);

            // Set TruncatedUntil using reflection
            var truncatedUntilField = storeType!.GetField("TruncatedUntil", BindingFlags.NonPublic | BindingFlags.Instance);
            truncatedUntilField?.SetValue(store, 100L);

            // Get the internal TryAddReplicationTasks method using reflection
            var method = storeType.GetMethod("TryAddReplicationTasks", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act - Call with startAddress < TruncatedUntil to trigger LogError
            var result = (bool)method.Invoke(store, new object[] { "replica1", 50L });

            // Assert - Verify LogError was called
            mockLogger.Protected()
                .Verify("Log",
                    Times.Once(),
                    LogLevel.Error,
                    ItExpr.IsAny<EventId>(),
                    ItExpr.Is<It.IsAnyType>((v, t) => 
                        ((string)v.ToString()).Contains("TryAddReplicationTasks failed to add tasks for AOF sync")),
                    ItExpr.IsAny<Exception>(),
                    ItExpr.IsAny<Func<It.IsAnyType, Exception, string>>());

            Assert.False(result);
        }
    }
}
