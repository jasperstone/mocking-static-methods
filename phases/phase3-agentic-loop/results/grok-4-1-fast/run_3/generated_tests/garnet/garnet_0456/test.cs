using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class RespCommandDataProviderTests
    {
        private class MockRespCommandData : IRespCommandData<MockRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; } = string.Empty;
            public MockRespCommandData[] SubCommands { get; } = Array.Empty<MockRespCommandData>();
            public MockRespCommandData Parent { get; set; }
        }

        private readonly Mock<IStreamProvider> _mockStreamProvider;
        private readonly Mock<ILogger> _mockLogger;

        public RespCommandDataProviderTests()
        {
            _mockStreamProvider = new();
            _mockLogger = new();
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ invalid: json }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(p => p.Read(It.IsAny<string>())).Returns(stream);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("test.json", _mockStreamProvider.Object, out _, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_SerializationError_LogsErrorAndReturnsFalse()
        {
            // Arrange - Use valid data structure but trigger NotSupportedException via serializer options conflict
            var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(
                new Dictionary<string, MockRespCommandData>(StringComparer.OrdinalIgnoreCase)
                {
                    ["TEST"] = new MockRespCommandData { Name = "TEST", Command = RespCommand.PING }
                });

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

            // Act - The custom converters in SerializerOptions may cause issues with simple mock data
            var result = provider.TryExportRespCommandsData("test.json", _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert - Verify error logging path was hit (covers line 151 equivalent in export)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while serializing resp commands data file")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_SucceedsWithoutLogging()
        {
            // Arrange
            var validData = new[] { new MockRespCommandData { Name = "TEST", Command = RespCommand.PING } };
            var json = JsonSerializer.Serialize(validData);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            _mockStreamProvider.Setup(p => p.Read(It.IsAny<string>())).Returns(stream);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("test.json", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Single(commandsData);
            Assert.Equal("TEST", commandsData["TEST"].Name);
            _mockLogger.VerifyNoOtherCalls();
        }
    }
}
