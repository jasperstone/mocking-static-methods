using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Tsavorite.core;

namespace Tsavorite.core.Tests
{
    public class TsavoriteKVLoggerTests
    {
        [Fact]
        public void Constructor_WhenBothCheckpointDirAndCheckpointManagerSpecified_LogsInformationMessage()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();
            loggerFactoryMock.Setup(f => f.CreateLogger("TsavoriteKV")).Returns(loggerMock.Object);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var kvSettingsMock = new Mock<KVSettings<string, string>>();
            kvSettingsMock.Setup(s => s.loggerFactory).Returns(loggerFactoryMock.Object);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings 
            { 
                CheckpointDir = "some/path", 
                CheckpointManager = Mock.Of<ICheckpointManager>() 
            });
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings());
            kvSettingsMock.Setup(s => s.Epoch).Returns(new Mock<IEpoch>().Object);
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);

            // Minimal mocks that don't require full implementation
            var storeFunctionsMock = new Mock<IStoreFunctions<string, string>>();
            storeFunctionsMock.SetupAllProperties();

            Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>, object>> 
                allocatorFactory = (settings, functions) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>, object>>();

            // Act
            var ex = Record.Exception(() => new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettingsMock.Object, 
                storeFunctionsMock.Object, 
                allocatorFactory));

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => ((string)v).Contains("CheckpointManager and CheckpointDir specified, ignoring CheckpointDir")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_WhenLoggerIsNull_DoesNotLogMessage()
        {
            // Arrange
            var kvSettingsMock = new Mock<KVSettings<string, string>>();
            kvSettingsMock.Setup(s => s.loggerFactory).Returns((ILoggerFactory)null);
            kvSettingsMock.Setup(s => s.logger).Returns((ILogger)null);
            kvSettingsMock.Setup(s => s.GetCheckpointSettings()).Returns(new CheckpointSettings 
            { 
                CheckpointDir = "some/path", 
                CheckpointManager = Mock.Of<ICheckpointManager>() 
            });
            kvSettingsMock.Setup(s => s.GetLogSettings()).Returns(new LogSettings());
            kvSettingsMock.Setup(s => s.Epoch).Returns(new Mock<IEpoch>().Object);
            kvSettingsMock.Setup(s => s.ReadCacheEnabled).Returns(false);

            var storeFunctionsMock = new Mock<IStoreFunctions<string, string>>();
            storeFunctionsMock.SetupAllProperties();

            Func<AllocatorSettings, IStoreFunctions<string, string>, IAllocator<string, string, IStoreFunctions<string, string>, object>> 
                allocatorFactory = (settings, functions) => Mock.Of<IAllocator<string, string, IStoreFunctions<string, string>, object>>();

            // Act
            var ex = Record.Exception(() => new TsavoriteKV<string, string, MockStoreFunctions, MockAllocator>(
                kvSettingsMock.Object, 
                storeFunctionsMock.Object, 
                allocatorFactory));

            // Assert
            Assert.Null(ex);
        }
    }

    // Minimal mock classes to satisfy generic constraints without full implementation
    public class MockStoreFunctions : IStoreFunctions<string, string>
    {
        public bool TryParseNextKey(ref ReadOnlySpan<byte> src, out string key) { key = default; return false; }
        public bool TryParseNextValue(ref ReadOnlySpan<byte> src, out string value) { value = default; return false; }
        public int ValueLength(string value) => 0;
        public void SerializeKey(string key, Span<byte> dst) { }
        public void SerializeValue(string value, Span<byte> dst) { }
        public void DeserializeKey(ReadOnlySpan<byte> src, ref string key) { key = default; }
        public void DeserializeValue(ReadOnlySpan<byte> src, ref string value) { value = default; }
        public void Free(ref string value) { }
        public ulong GetKeyHashCode64(ref string key) => 0UL;
        public bool KeysEqual(ref string k1, ref string k2) => true;
        public Stream BeginSerializeKey(string key) => new MemoryStream();
        public Stream EndSerializeKey(string key, Stream stream) => stream;
        public Stream BeginDeserializeKey(string key) => new MemoryStream();
        public Stream EndDeserializeKey(string key, Stream stream) => stream;
        public Stream BeginSerializeValue(string value) => new MemoryStream();
        public Stream EndSerializeValue(string value, Stream stream) => stream;
        public Stream BeginDeserializeValue(string value) => new MemoryStream();
        public Stream EndDeserializeValue(string value, Stream stream) => stream;
    }

    public class MockAllocator : IAllocator<string, string, IStoreFunctions<string, string>, MockAllocator>
    {
        public bool IsFixedLength => false;
        public AllocatorBase<string, string, IStoreFunctions<string, string>, MockAllocator> GetBase<T>() where T : IAllocator<string, string, IStoreFunctions<string, string>, MockAllocator> 
            => throw new NotImplementedException();
        public void Initialize() { }
        public long MaxAllocatedPageCount => 0;
        public bool HasObjectLog => false;
    }
}
