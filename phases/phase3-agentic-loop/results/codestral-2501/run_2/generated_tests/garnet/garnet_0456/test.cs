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

namespace Garnet.server.Tests
{
    public class RespCommandsDataProviderTests
    {
        private readonly Mock<IStreamProvider> _mockStreamProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly DefaultRespCommandsDataProvider<TestRespCommandData> _provider;

        public RespCommandsDataProviderTests()
        {
            _mockStreamProvider = new Mock<IStreamProvider>();
            _mockLogger = new Mock<ILogger>();
            _provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
        {
            // Arrange
            var json = "[{\"Name\":\"COMMAND1\",\"Command\":\"COMMAND1\",\"SubCommands\":null},{\"Name\":\"COMMAND2\",\"Command\":\"COMMAND2\",\"SubCommands\":null}]";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData("path", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

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
            _mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData("path", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            _mockLogger.Verify(logger => logger.Log(
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
                { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.COMMAND1 } },
                { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.COMMAND2 } }
            });

            // Act
            var result = _provider.TryExportRespCommandsData("path", _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert
            Assert.True(result);
            _mockStreamProvider.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_InvalidData_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var commandsData = new ReadOnlyDictionary<string, TestRespCommandData>(new Dictionary<string, TestRespCommandData>
            {
                { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.COMMAND1 } },
                { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.COMMAND2 } }
            });

            _mockStreamProvider.Setup(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(new NotSupportedException());

            // Act
            var result = _provider.TryExportRespCommandsData("path", _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsError()
        {
            // Arrange
            var json = "invalid json";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData("path", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<JsonException>(),
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
        COMMAND1,
        COMMAND2
    }
}
