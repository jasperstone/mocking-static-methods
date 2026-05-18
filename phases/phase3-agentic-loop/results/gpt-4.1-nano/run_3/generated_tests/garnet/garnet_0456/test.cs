using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        private class DummyData : IRespCommandData<DummyData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public DummyData[] SubCommands { get; init; }
            public DummyData Parent { get; set; }
        }

        private class DummyStreamProvider : IStreamProvider
        {
            private readonly MemoryStream _stream;

            public DummyStreamProvider(string content)
            {
                _stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            }

            public Stream Read(string path) => _stream;

            public void Write(string path, byte[] data)
            {
                // No-op for write
            }
        }

        [Fact]
        public void TryImportRespCommandsData_JsonException_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var streamProvider = new DummyStreamProvider(invalidJson);
            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProvider, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<JsonException>(), It.Is<string>(s => s.Contains("Path: dummyPath"))),
                Times.Once);
        }
    }
}
