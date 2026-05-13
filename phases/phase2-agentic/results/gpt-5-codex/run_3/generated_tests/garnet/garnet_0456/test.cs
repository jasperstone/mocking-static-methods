using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Garnet.server.Resp;
using Moq;
using Xunit;

namespace Garnet.Tests.Server.Resp
{
    public class RespCommandDataProviderTests
    {
        private sealed class TestStreamProvider : RespCommandDataProvider<RespCommandData>.IStreamProvider
        {
            private readonly Stream _streamToReturn;

            public TestStreamProvider(Stream streamToReturn)
            {
                _streamToReturn = streamToReturn;
            }

            public Stream Read(string path) => _streamToReturn;

            public void Write(string path, ReadOnlySpan<byte> data) =>
                throw new NotImplementedException();
        }

        [Fact]
        public void TryImportRespCommandsData_WhenJsonExceptionThrown_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = Encoding.UTF8.GetBytes("{ invalid json");
            var stream = new MemoryStream(invalidJson);
            var streamProvider = new TestStreamProvider(stream);
            var provider = new RespCommandDataProvider<RespCommandData>();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = provider.TryImportRespCommandsData("test-path", streamProvider, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("An error occurred while parsing resp command data file (Path: {path}).")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
