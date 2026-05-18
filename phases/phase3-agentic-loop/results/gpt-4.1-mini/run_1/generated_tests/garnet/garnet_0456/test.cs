using System;
using System.Collections.Generic;
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
            public TestRespCommandData[] SubCommands { get; } = null!;
            public TestRespCommandData Parent { get; set; }
        }

        private class TestStreamProvider : IStreamProvider
        {
            private readonly Stream _stream;
            private readonly Action<string, byte[]>? _writeAction;

            public TestStreamProvider(Stream stream, Action<string, byte[]>? writeAction = null)
            {
                _stream = stream;
                _writeAction = writeAction;
            }

            public Stream Read(string path)
            {
                _stream.Position = 0;
                return _stream;
            }

            public void Write(string path, byte[] data)
            {
                _writeAction?.Invoke(path, data);
            }
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndPopulatesCommandsData()
        {
            // Arrange
            var json = "[{\"Name\":\"cmd1\",\"Command\":0},{\"Name\":\"cmd2\",\"Command\":0,\"SubCommands\":[{\"Name\":\"sub1\",\"Command\":0}]}]";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var streamProvider = new TestStreamProvider(stream);

            var loggerMock = new Mock<ILogger>();

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("path", streamProvider, out var commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Equal(2, commandsData.Count);
            Assert.Contains("cmd1", commandsData.Keys);
            Assert.Contains("cmd2", commandsData.Keys);
            var cmd2 = commandsData["cmd2"];
            Assert.NotNull(cmd2.SubCommands);
            Assert.Single(cmd2.SubCommands);
            Assert.Equal(cmd2, cmd2.SubCommands[0].Parent);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            var streamProvider = new TestStreamProvider(stream);

            var loggerMock = new Mock<ILogger>();

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("path", streamProvider, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<JsonException>(),
                    "An error occurred while parsing resp command data file (Path: {path}).",
                    "path"),
                Times.Once);
        }
    }
}
