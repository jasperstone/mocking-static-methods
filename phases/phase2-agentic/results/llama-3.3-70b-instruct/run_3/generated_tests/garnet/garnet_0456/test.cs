using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class RespCommandDataProviderTests
    {
        [Fact]
        public async Task TryImportRespCommandsData_JsonException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var streamProviderMock = new Mock<IStreamProvider>();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid JSON"));
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);
            var path = "path";

            // Act
            var provider = new DefaultRespCommandsDataProvider<RespCommandData>();
            var result = provider.TryImportRespCommandsData(path, streamProviderMock.Object, out _, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task TryExportRespCommandsData_NotSupportedException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var streamProviderMock = new Mock<IStreamProvider>();
            var commandsData = new Dictionary<string, RespCommandData> { { "command", new RespCommandData() } };
            var path = "path";

            // Act
            var provider = new DefaultRespCommandsDataProvider<RespCommandData>();
            var result = provider.TryExportRespCommandsData(path, streamProviderMock.Object, commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        private class RespCommandData : IRespCommandData<RespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public RespCommandData[] SubCommands { get; }
            public RespCommandData Parent { get; set; }
        }
    }
}
