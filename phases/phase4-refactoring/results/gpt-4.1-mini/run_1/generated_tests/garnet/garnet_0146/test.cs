using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogCheckpointEntry_LogsExpectedMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var entryType = Type.GetType("Garnet.cluster.CheckpointEntry, Garnet");
            Assert.NotNull(entryType);

            var entry = Activator.CreateInstance(entryType);
            Assert.NotNull(entry);

            var metadataField = entryType.GetField("metadata", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.NotNull(metadataField);

            var metadata = metadataField.GetValue(entry);
            Assert.NotNull(metadata);

            var metadataType = metadata.GetType();

            // Set some properties on metadata to non-default values
            var storeVersionProp = metadataType.GetProperty("storeVersion");
            var storeHlogTokenProp = metadataType.GetProperty("storeHlogToken");
            var storeIndexTokenProp = metadataType.GetProperty("storeIndexToken");
            var storeCheckpointCoveredAofAddressProp = metadataType.GetProperty("storeCheckpointCoveredAofAddress");
            var storePrimaryReplIdProp = metadataType.GetProperty("storePrimaryReplId");

            var objectStoreVersionProp = metadataType.GetProperty("objectStoreVersion");
            var objectStoreHlogTokenProp = metadataType.GetProperty("objectStoreHlogToken");
            var objectStoreIndexTokenProp = metadataType.GetProperty("objectStoreIndexToken");
            var objectCheckpointCoveredAofAddressProp = metadataType.GetProperty("objectCheckpointCoveredAofAddress");
            var objectStorePrimaryReplIdProp = metadataType.GetProperty("objectStorePrimaryReplId");

            storeVersionProp.SetValue(metadata, 1L);
            storeHlogTokenProp.SetValue(metadata, Guid.NewGuid());
            storeIndexTokenProp.SetValue(metadata, Guid.NewGuid());
            storeCheckpointCoveredAofAddressProp.SetValue(metadata, 12345L);
            storePrimaryReplIdProp.SetValue(metadata, "primaryReplId");

            objectStoreVersionProp.SetValue(metadata, 2L);
            objectStoreHlogTokenProp.SetValue(metadata, Guid.NewGuid());
            objectStoreIndexTokenProp.SetValue(metadata, Guid.NewGuid());
            objectCheckpointCoveredAofAddressProp.SetValue(metadata, 67890L);
            objectStorePrimaryReplIdProp.SetValue(metadata, "objectPrimaryReplId");

            var lockField = entryType.GetField("_lock", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.NotNull(lockField);
            var lockValue = lockField.GetValue(entry);
            Assert.NotNull(lockValue);

            // Act
            var loggerExtensionsType = Type.GetType("Garnet.cluster.CheckpointEntryExtensions, Garnet");
            Assert.NotNull(loggerExtensionsType);

            var logCheckpointEntryMethod = loggerExtensionsType.GetMethod("LogCheckpointEntry", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            Assert.NotNull(logCheckpointEntryMethod);

            logCheckpointEntryMethod.Invoke(null, new object[] { mockLogger.Object, LogLevel.Trace, "TestMessage", entry });

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestMessage")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
