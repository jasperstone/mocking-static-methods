using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Garnet.common;
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
        public void TryImportRespCommandsData_JsonException_LogsError()
        {
            // Arrange
            var invalidJson = "invalid json";
            var path = "testPath";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
        {
            // Arrange
            var validJson = "[{\"Name\": \"TestCommand\", \"Command\": 0}]";
            var path = "testPath";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(validJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Single(commandsData);
            Assert.Equal("TestCommand", commandsData["TestCommand"].Name);
        }

        [Fact]
        public void TryExportRespCommandsData_NotSupportedException_LogsError()
        {
            // Arrange
            var path = "testPath";
            var commandsData = new ReadOnlyDictionary<string, TestRespCommandData>(new Dictionary<string, TestRespCommandData>
            {
                { "TestCommand", new TestRespCommandData { Name = "TestCommand", Command = RespCommand.TestCommand } }
            });
            _mockStreamProvider.Setup(sp => sp.Write(path, It.IsAny<byte[]>())).Throws<NotSupportedException>();

            // Act
            var result = _provider.TryExportRespCommandsData(path, _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_ValidData_ReturnsTrue()
        {
            // Arrange
            var path = "testPath";
            var commandsData = new ReadOnlyDictionary<string, TestRespCommandData>(new Dictionary<string, TestRespCommandData>
            {
                { "TestCommand", new TestRespCommandData { Name = "TestCommand", Command = RespCommand.TestCommand } }
            });

            // Act
            var result = _provider.TryExportRespCommandsData(path, _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert
            Assert.True(result);
            _mockStreamProvider.Verify(sp => sp.Write(path, It.IsAny<byte[]>()), Times.Once);
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
        TestCommand
    }
}
