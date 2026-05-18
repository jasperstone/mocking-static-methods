using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace Garnet.Tests
{
    public class RespCommandDataProviderTests
    {
        [Fact]
        public async Task TryImportRespCommandsData_ValidJson_ReturnsTrue()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<IRespCommandData>();

            var json = "{\"Name\":\"TestCommand\",\"Command\":0}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = respCommandDataProvider.TryImportRespCommandsData("test.json", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Single(commandsData);
        }

        [Fact]
        public async Task TryImportRespCommandsData_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<IRespCommandData>();

            var json = "Invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = respCommandDataProvider.TryImportRespCommandsData("test.json", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task TryExportRespCommandsData_ValidData_ReturnsTrue()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<IRespCommandData>();

            var commandsData = new Dictionary<string, IRespCommandData>
            {
                { "TestCommand", new RespCommandInfo { Name = "TestCommand", Command = RespCommand.Default } }
            };

            // Act
            var result = respCommandDataProvider.TryExportRespCommandsData("test.json", streamProviderMock.Object, commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TryExportRespCommandsData_InvalidData_ReturnsFalse()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<IRespCommandData>();

            var commandsData = new Dictionary<string, IRespCommandData>
            {
                { "TestCommand", new RespCommandInfo { Name = "TestCommand", Command = (RespCommand)1000 } }
            };

            // Act
            var result = respCommandDataProvider.TryExportRespCommandsData("test.json", streamProviderMock.Object, commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
