using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        private class TestRespCommandData : IRespCommandData<TestRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public TestRespCommandData[] SubCommands { get; }
            public TestRespCommandData Parent { get; set; }

            public TestRespCommandData(string name)
            {
                Name = name;
                SubCommands = Array.Empty<TestRespCommandData>();
            }
        }

        private class TestStreamProvider : IStreamProvider
        {
            private readonly Stream _readStream;
            private MemoryStream _writeStream;

            public TestStreamProvider(Stream readStream)
            {
                _readStream = readStream;
            }

            public Stream Read(string path)
            {
                _readStream.Position = 0;
                return _readStream;
            }

            public void Write(string path, byte[] data)
            {
                _writeStream = new MemoryStream(data);
            }

            public string GetWrittenData()
            {
                if (_writeStream == null) return null;
                _writeStream.Position = 0;
                using var reader = new StreamReader(_writeStream, Encoding.ASCII);
                return reader.ReadToEnd();
            }
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndPopulatesCommandsData()
        {
            // Arrange
            var validJson = "[{\"Name\":\"cmd1\",\"Command\":0}]";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
            var streamProvider = new TestStreamProvider(stream);
            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestRespCommandData>();
            IReadOnlyDictionary<string, TestRespCommandData> commandsData;

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProvider, out commandsData);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.True(commandsData.ContainsKey("cmd1"));
            Assert.Equal("cmd1", commandsData["cmd1"].Name);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            var streamProvider = new TestStreamProvider(stream);
            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestRespCommandData>();
            IReadOnlyDictionary<string, TestRespCommandData> commandsData;

            var mockLogger = new Mock<ILogger>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProvider, out commandsData, mockLogger.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<System.Text.Json.JsonException>(),
                    "An error occurred while parsing resp command data file (Path: {path}).",
                    "dummyPath"),
                Times.Once);
        }
    }
}
