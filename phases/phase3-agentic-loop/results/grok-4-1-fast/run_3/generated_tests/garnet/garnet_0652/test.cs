using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.core.Tests
{
    // Minimal mock to satisfy IStoreFunctions interface (just enough for constructor to run)
    public class MockStoreFunctions : IStoreFunctions<int, int>
    {
        public bool TryParseNextKey(ref ReadOnlySpan<byte> src, out int key) 
        {
            key = default;
            return false;
        }
        
        public long GetKeyHashCode64(ref int key) => (long)key.GetHashCode();
        public int GetKeyHashCode(int key) => key.GetHashCode();
        
        public bool KeysEqual(ref int a, ref int b) => a.Equals(b);
        public int CompareKeys(int a, int b) => a.CompareTo(b);
        
        public uint GetKeyLength(int key) => sizeof(int);
        public int GetValueLength(int value) => sizeof(int);
        
        public bool TryParseNextValue(ref ReadOnlySpan<byte> src, out int value)
        {
            value = default;
            return false;
        }
        
        public void SerializeKey(int key, Span<byte> dst) 
        {
            BitConverter.TryWriteBytes(dst, key);
        }
        
        public void SerializeValue(int value, Span<byte> dst)
        {
            BitConverter.TryWriteBytes(dst, value);
        }
        
        public void SingleWriterSerializeKey(int key, Span<byte> dst) => SerializeKey(key, dst);
        public void SingleWriterSerializeValue(int value, Span<byte> dst) => SerializeValue(value, dst);
        
        public IObjectSerializer BeginSerializeKey(Stream stream) => throw new NotImplementedException();
        public IObjectSerializer EndSerializeKey(IObjectSerializer serializer) => throw new NotImplementedException();
        public IObjectDeserializer BeginDeserializeKey(Stream stream) => throw new NotImplementedException();
        public IObjectDeserializer EndDeserializeKey(IObjectDeserializer deserializer) => throw new NotImplementedException();
        public IObjectSerializer BeginSerializeValue(Stream stream) => throw new NotImplementedException();
        public IObjectSerializer EndSerializeValue(IObjectSerializer serializer) => throw new NotImplementedException();
        public IObjectDeserializer BeginDeserializeValue(Stream stream) => throw new NotImplementedException();
        public IObjectDeserializer EndDeserializeValue(IObjectDeserializer deserializer) => throw new NotImplementedException();
        
        public void DisposeRecord(ref int key, ref int value, DisposeReason reason, int threadID) { }
    }

    // Minimal mock for IAllocator
    public class MockAllocator : IAllocator<int, int, MockStoreFunctions>
    {
        public AllocatorBase<int, int, MockStoreFunctions, MockAllocator> GetBase<TAllocator>() where TAllocator : IAllocator<int, int, MockStoreFunctions>
            => throw new NotImplementedException("Mock - constructor doesn't reach here");
        
        public bool IsFixedLength => true;
        public bool HasObjectLog => false;
        public long GetMaxValidAddress() => 0;
    }

    public class TsavoriteKVTests
    {
        [Fact]
        public void Constructor_WhenCheckpointDirAndCheckpointManagerBothSpecified_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new Mock<KVSettings<int, int>>();
            kvSettings.Setup(s => s.logger).Returns<ILogger?>(null);
            kvSettings.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettings.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings 
            { 
                CheckpointDir = "test-dir",
                CheckpointManager = new Mock<ICheckpointManager>().Object 
            });

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, MockStoreFunctions, MockAllocator> allocatorFactory = (_, _) => new MockAllocator();

            // Act
            var ex = Record.Exception(() => 
                new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(
                    kvSettings.Object, storeFunctions, allocatorFactory));

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    "CheckpointManager and CheckpointDir specified, ignoring CheckpointDir"),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenCheckpointDirNull_DoesNotLogInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new Mock<KVSettings<int, int>>();
            kvSettings.Setup(s => s.logger).Returns<ILogger?>(null);
            kvSettings.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettings.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings 
            { 
                CheckpointDir = null,
                CheckpointManager = new Mock<ICheckpointManager>().Object 
            });

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, MockStoreFunctions, MockAllocator> allocatorFactory = (_, _) => new MockAllocator();

            // Act
            var ex = Record.Exception(() => 
                new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(
                    kvSettings.Object, storeFunctions, allocatorFactory));

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("CheckpointManager and CheckpointDir"))),
                Times.Never);
        }

        [Fact]
        public void Constructor_WhenCheckpointManagerNull_DoesNotLogInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettings = new Mock<KVSettings<int, int>>();
            kvSettings.Setup(s => s.logger).Returns<ILogger?>(null);
            kvSettings.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettings.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings 
            { 
                CheckpointDir = "test-dir",
                CheckpointManager = null 
            });

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, MockStoreFunctions, MockAllocator> allocatorFactory = (_, _) => new MockAllocator();

            // Act
            var ex = Record.Exception(() => 
                new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(
                    kvSettings.Object, storeFunctions, allocatorFactory));

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("CheckpointManager and CheckpointDir"))),
                Times.Never);
        }
    }
}
