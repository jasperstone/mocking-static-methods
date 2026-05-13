using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class DefaultRespCommandsDataProviderTests
    {
        private class TestRespCommandData : IRespCommandData<TestRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public TestRespCommandData[] SubCommands { get; } = Array.Empty<TestRespCommandData>();
            public TestRespCommandData Parent { get; set; }
        }

        private class TestStreamProvider : IStreamProvider
        {
            private readonly MemoryStream _readStream;
            public byte[] WrittenData { get; private set; }
            public string WrittenPath { get; private set; }

            public TestStreamProvider(string readContent)
            {
                _readStream = new MemoryStream(Encoding.UTF8.GetBytes(readContent));
            }

            public Stream Read(string path)
            {
                return _readStream;
            }

            public void Write(string path, byte[] data)
            {
                WrittenPath = path;
                WrittenData = data;
            }
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndPopulatesCommandsData()
        {
            var json = "[{\"Name\":\"cmd1\",\"Command\":0}]";
            var streamProvider = new TestStreamProvider(json);
            var provider = DefaultRespCommandsDataProvider<TestRespCommandData>.Instance;

            var result = provider.TryImportRespCommandsData("path", streamProvider, out var commandsData);

            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.True(commandsData.ContainsKey("cmd1"));
            Assert.Equal("cmd1", commandsData["cmd1"].Name);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            var invalidJson = "invalid json";
            var streamProvider = new TestStreamProvider(invalidJson);
            var provider = DefaultRespCommandsDataProvider<TestRespCommandData>.Instance;

            var mockLogger = new Mock<ILogger>();

            var result = provider.TryImportRespCommandsData("path", streamProvider, out var commandsData, mockLogger.Object);

            Assert.False(result);
            Assert.Null(commandsData);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
