using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class RespCommandDataProviderTests
    {
        private readonly Mock<IStreamProvider> _mockStreamProvider;
        private readonly Mock<ILogger> _mockLogger;

        public RespCommandDataProviderTests()
        {
            _mockStreamProvider = new Mock<IStreamProvider>();
            _mockLogger = new Mock<ILogger>();
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var path = "invalid.json";
            var invalidJson = "invalid json";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();
            IReadOnlyDictionary<string, MockRespCommandData> result;

            // Act
            var success = provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out result, _mockLogger.Object);

            // Assert
            Assert.False(success);
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while parsing resp command data file (Path: " + path + ")")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception>>()),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_NotSupportedException_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var path = "export.json";
            var problematicData = new MockRespCommandData 
            { 
                Name = "TEST", 
                Command = RespCommand.PING, 
                ProblematicProperty = new object() 
            };
            var commandsData = new Dictionary<string, MockRespCommandData> { ["TEST"] = problematicData };
            var readOnlyDict = new ReadOnlyDictionary<string, MockRespCommandData>(commandsData);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();
            _mockStreamProvider.Setup(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Verifiable();

            // Act
            var success = provider.TryExportRespCommandsData(path, _mockStreamProvider.Object, readOnlyDict, _mockLogger.Object);

            // Assert
            Assert.False(success);
            _mockStreamProvider.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while serializing resp command data file (Path: " + path + ")")),
                    It.IsAny<NotSupportedException>(),
                    It.IsAny<Func<It.IsAnyType, Exception>>()),
                Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_NullLogger_DoesNotThrow()
        {
            // Arrange
            var path = "invalid.json";
            var invalidJson = "{invalid}";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();
            IReadOnlyDictionary<string, MockRespCommandData> result;

            // Act
            var success = provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out result, null);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_Succeeds()
        {
            // Arrange
            var path = "valid.json";
            var json = "[{\"Name\":\"TEST\",\"Command\":0}]";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();
            IReadOnlyDictionary<string, MockRespCommandData> result;

            // Act
            var success = provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out result, _mockLogger.Object);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("TEST", result["test"].Name);
        }
    }

    public class MockRespCommandData : IRespCommandData<MockRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; } = string.Empty;
        public MockRespCommandData[] SubCommands { get; init; } = Array.Empty<MockRespCommandData>();
        public MockRespCommandData Parent { get; set; } = null!;
        public object? ProblematicProperty { get; set; }
    }
}
