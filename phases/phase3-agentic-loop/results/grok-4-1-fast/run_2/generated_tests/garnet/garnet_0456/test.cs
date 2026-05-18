using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.ObjectModel;

namespace Garnet.server.Tests
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
        public void TryImportRespCommandsData_JsonException_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var path = "test.json";
            var invalidJson = "invalid json";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            var factory = new RespCommandsDataProviderFactory();
            var provider = factory.GetRespCommandsDataProvider<MockRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out _, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockStreamProvider.Verify(sp => sp.Read(path), Times.Once);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Path: {path}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_Success_DoesNotLogError()
        {
            // Arrange
            var path = "test.json";
            var mockData = new MockRespCommandData { Name = "TEST" };
            var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(
                new Dictionary<string, MockRespCommandData> { ["TEST"] = mockData },
                StringComparer.OrdinalIgnoreCase);

            _mockStreamProvider.Setup(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()));

            var factory = new RespCommandsDataProviderFactory();
            var provider = factory.GetRespCommandsDataProvider<MockRespCommandData>();

            // Act
            var result = provider.TryExportRespCommandsData(path, _mockStreamProvider.Object, commandsData, _mockLogger.Object);

            // Assert
            Assert.True(result);
            _mockStreamProvider.Verify(sp => sp.Write(path, It.IsAny<byte[]>()), Times.Once);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }

    // Minimal implementation for testing
    public class MockRespCommandData : IRespCommandData<MockRespCommandData>
    {
        public RespCommand Command { get; init; } = default;
        public string Name { get; init; } = string.Empty;
        public MockRespCommandData[] SubCommands { get; } = Array.Empty<MockRespCommandData>();
        public MockRespCommandData Parent { get; set; } = null!;
    }
}
