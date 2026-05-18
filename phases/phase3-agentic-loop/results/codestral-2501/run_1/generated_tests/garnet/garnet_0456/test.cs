using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        private readonly Mock<IStreamProvider> _streamProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly DefaultRespCommandsDataProvider<TestRespCommandData> _provider;

        public RespCommandsDataProviderTests()
        {
            _streamProviderMock = new Mock<IStreamProvider>();
            _loggerMock = new Mock<ILogger>();
            _provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
        {
            // Arrange
            var json = "[{\"Name\": \"COMMAND1\", \"Command\": 1}, {\"Name\": \"COMMAND2\", \"Command\": 2}]";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData("path", _streamProviderMock.Object, out var commandsData, _loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Equal(2, commandsData.Count);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var json = "invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData("path", _streamProviderMock.Object, out var commandsData, _loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_ValidData_ReturnsTrue()
        {
            // Arrange
            var commandsData = new ReadOnlyDictionary<string, TestRespCommandData>(new Dictionary<string, TestRespCommandData>
            {
                { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.Command1 } },
                { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.Command2 } }
            });

            // Act
            var result = _provider.TryExportRespCommandsData("path", _streamProviderMock.Object, commandsData, _loggerMock.Object);

            // Assert
            Assert.True(result);
            _streamProviderMock.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_InvalidData_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var commandsData = new ReadOnlyDictionary<string, TestRespCommandData>(new Dictionary<string, TestRespCommandData>
            {
                { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.Command1 } },
                { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.Command2 } }
            });

            _streamProviderMock.Setup(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Throws<NotSupportedException>();

            // Act
            var result = _provider.TryExportRespCommandsData("path", _streamProviderMock.Object, commandsData, _loggerMock.Object);

            // Assert
            Assert.False(result);
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class TestRespCommandData : IRespCommandData<TestRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; }
        public TestRespCommandData[] SubCommands { get; set; }
        public TestRespCommandData Parent { get; set; }
    }

    public enum RespCommand
    {
        Command1,
        Command2
    }
}
