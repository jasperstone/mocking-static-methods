using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class RespCommandDataProviderTests
    {
        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

            var json = "[{\"Name\":\"Command1\",\"Command\":0},{\"Name\":\"Command2\",\"Command\":1}]";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = respCommandDataProvider.TryImportRespCommandsData("path", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Equal(2, commandsData.Count);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

            var json = "Invalid json";
            var stream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = respCommandDataProvider.TryImportRespCommandsData("path", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_ValidData_ReturnsTrue()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

            var commandsData = new Dictionary<string, TestRespCommandData>
            {
                { "Command1", new TestRespCommandData { Name = "Command1", Command = RespCommand.Command1 } },
                { "Command2", new TestRespCommandData { Name = "Command2", Command = RespCommand.Command2 } }
            };

            // Act
            var result = respCommandDataProvider.TryExportRespCommandsData("path", streamProviderMock.Object, commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            streamProviderMock.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_InvalidData_ReturnsFalse()
        {
            // Arrange
            var streamProviderMock = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

            var commandsData = new Dictionary<string, TestRespCommandData>
            {
                { "Command1", new TestRespCommandData { Name = "Command1", Command = RespCommand.Command1 } },
                { "Command2", new TestRespCommandData { Name = "Command2", Command = (RespCommand)100 } } // Invalid command
            };

            // Act
            var result = respCommandDataProvider.TryExportRespCommandsData("path", streamProviderMock.Object, commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }

    public class TestRespCommandData : IRespCommandData
    {
        public int Command { get; init; }
        public string Name { get; init; }
    }

    public enum RespCommand
    {
        Command1 = 0,
        Command2 = 1
    }
}
