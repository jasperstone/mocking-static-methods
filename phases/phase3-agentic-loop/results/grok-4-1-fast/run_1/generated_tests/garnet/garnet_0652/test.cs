using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Threading;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVTests
    {
        [Fact]
        public void Constructor_LogsInformation_WhenCheckpointDirAndCheckpointManagerBothSpecified()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<KVSettings<string, string>>();
            kvSettingsMock.Setup(s => s.Epoch).Returns(new Mock<IEpoch>().Object);
            kvSettingsMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            
            var checkpointSettings = new MockCheckpointSettings { CheckpointDir = "some/dir", CheckpointManager = new Mock<IDisposable>().Object };
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, IStoreFunctions<string, string>, MockAllocator> allocatorFactory = 
                (settings, functions) => new MockAllocator();

            // Act
            var tsavorite = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettingsMock.Object, 
                storeFunctions, 
                allocatorFactory);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(v => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_DoesNotLogInformation_WhenCheckpointDirIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var kvSettingsMock = new Mock<KVSettings<string, string>>();
            kvSettingsMock.Setup(s => s.Epoch).Returns(new Mock<IEpoch>().Object);
            kvSettingsMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            
            var checkpointSettings = new MockCheckpointSettings { CheckpointDir = null, CheckpointManager = new Mock<IDisposable>().Object };
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(checkpointSettings);

            var storeFunctions = new MockStoreFunctions();
            Func<AllocatorSettings, IStoreFunctions<string, string>, MockAllocator> allocatorFactory =
                (settings, functions) => new MockAllocator();

            // Act
            var tsavorite = new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettingsMock.Object,
                storeFunctions,
                allocatorFactory);

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>(v => v.ToString().Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    public class MockCheckpointSettings
    {
        public string CheckpointDir { get; set; }
        public object CheckpointManager { get; set; }
        public int ThrottleCheckpointFlushDelayMs => -1;
        public bool RemoveOutdated => false;
    }

    public class MockStoreFunctions : IStoreFunctions<string, string>
    {
        public ulong GetKeyHashCode64(ref string key) => 0;
        public bool KeysEqual(ref string key1, ref string key2) => true;
        public bool TryParseNextKey(ref ReadOnlySpan<byte> src, out string key) { key = default; return false; }
        public bool TryParseNextValue(ref ReadOnlySpan<byte> src, out string value) { value = default; return false; }
        public bool TryParseNextKeyValue(ref ReadOnlySpan<byte> src, out string key, out string value) { key = default; value = default; return false; }
        public int ValueLength(string value) => 0;
        public Span<byte> BeginSerializeKey(Stream dest) => default;
        public void EndSerializeKey(Stream dest, in string key) { }
        public string BeginDeserializeKey(Stream src) => null;
        public void EndDeserializeKey(Stream src) { }
        public Span<byte> BeginSerializeValue(Stream dest) => default;
        public void EndSerializeValue(Stream dest, in string value) { }
        public string BeginDeserializeValue(Stream src) => null;
        public void EndDeserializeValue(Stream src) { }
        public void DisposeRecord(ref string key, ref string value, DisposeReason reason, int size) { }
        public int GetKeyLength(ref string key) => 0;
    }

    public class MockAllocator : IAllocator<string, string, IStoreFunctions<string, string>>
    {
        public bool IsFixedLength => false;
        public AllocatorBase<string, string, IStoreFunctions<string, string>, MockAllocator> GetBase<TAllocator>() where TAllocator : IAllocator<string, string, IStoreFunctions<string, string>> 
            => throw new NotImplementedException("Not needed for constructor logging test");
        public long GetMaxValidAddress() => 0;
        public void Initialize() { }
        // Minimal implementations for other required members
        public string GetAndInitializeValue(long valueOffset, long allocatorAddress) => default;
        public int GetRMWCopyDestinationRecordSize<TInput, TVariableLengthInput>(ref string key, ref TInput input, ref string value, ref RecordInfo recordInfo, TVariableLengthInput variableLengthInput) => 0;
        public int GetRMWInitialRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref TInput input, TSessionFunctionsWrapper sessionFunctions) => 0;
        public int GetUpsertRecordSize<TInput, TSessionFunctionsWrapper>(ref string key, ref string value, ref TInput input, TSessionFunctionsWrapper sessionFunctions) => 0;
    }
}
