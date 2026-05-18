using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

        [Fact]
        public void TryImportRespCommandsData_ShouldLogErrorAndReturnFalse_OnJsonException()
        {
            // Arrange
            var mockStreamProvider = new Mock<IStreamProvider>();
            var mockStream = new MemoryStream();
            var jsonContent = "[{ \"Name\": \"Test\", \"Command\": 0, \"SubCommands\": null }]"; // invalid JSON for DummyData
            var writer = new StreamWriter(mockStream);
            writer.Write(jsonContent);
            writer.Flush();
            mockStream.Position = 0;
            mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(mockStream);

            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", mockStreamProvider.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<JsonException>(), It.Is<string>(s => s.Contains("Path: dummyPath"))),
                Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_ShouldPopulateCommandsData_OnSuccess()
        {
            // Arrange
            var data = new DummyData
            {
                Name = "Test",
                Command = RespCommand.PING,
                SubCommands = null
            };
            var json = JsonSerializer.Serialize(new[] { data }, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            });
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;

            var mockStreamProvider = new Mock<IStreamProvider>();
            mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", mockStreamProvider.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Contains("Test", commandsData);
            var command = commandsData["Test"];
            Assert.Equal("Test", command.Name);
        }

        [Fact]
        public void TryExportRespCommandsData_ShouldLogErrorAndReturnFalse_OnNotSupportedException()
        {
            // Arrange
            var commands = new Dictionary<string, DummyData>(StringComparer.OrdinalIgnoreCase)
            {
                { "Test", new DummyData { Name = "Test", Command = RespCommand.PING } }
            };
            var mockStreamProvider = new Mock<IStreamProvider>();
            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Force JsonSerializer.Serialize to throw
            var originalSerialize = JsonSerializer.Serialize;
            JsonSerializer.Serialize = (obj, options) => throw new NotSupportedException();

            // Act
            var result = provider.TryExportRespCommandsData("dummyPath", mockStreamProvider.Object, commands, loggerMock.Object);

            // Reset
            JsonSerializer.Serialize = originalSerialize;

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<NotSupportedException>(), It.Is<string>(s => s.Contains("Path: dummyPath"))),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_ShouldWriteData_ToStreamProvider()
        {
            // Arrange
            var commands = new Dictionary<string, DummyData>(StringComparer.OrdinalIgnoreCase)
            {
                { "Alpha", new DummyData { Name = "Alpha", Command = RespCommand.PING } },
                { "Beta", new DummyData { Name = "Beta", Command = RespCommand.PING } }
            };
            var mockStreamProvider = new Mock<IStreamProvider>();
            byte[] capturedData = null;
            mockStreamProvider.Setup(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((path, data) => capturedData = data);
            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Act
            var result = provider.TryExportRespCommandsData("dummyPath", mockStreamProvider.Object, commands, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(capturedData);
            Assert.NotEmpty(capturedData);
        }
    }
}
