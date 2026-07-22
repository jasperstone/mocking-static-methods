using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    // Minimal implementation satisfying generic constraints
    public class FakeRespCommandData : IRespCommandData<FakeRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; } = string.Empty;
        public FakeRespCommandData[] SubCommands { get; } = Array.Empty<FakeRespCommandData>();
        public FakeRespCommandData Parent { get; set; } = null!;
    }

    public class RespCommandsDataProviderTests
    {
        private readonly Mock<IStreamProvider> _mockStreamProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly string _testPath = "test.json";
        private readonly IRespCommandsDataProvider<FakeRespCommandData> _provider;

        public RespCommandsDataProviderTests()
        {
            _mockStreamProvider = new Mock<IStreamProvider>();
            _mockLogger = new Mock<ILogger>();
            _provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<FakeRespCommandData>();
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndPopulatesCommandsData()
        {
            // Arrange
            var validJson = "[]";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
            _mockStreamProvider.Setup(sp => sp.Read(_testPath)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(_testPath, _mockStreamProvider.Object, out var commandsData);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Empty(commandsData);
            _mockStreamProvider.Verify(sp => sp.Read(_testPath), Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(_testPath)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(_testPath, _mockStreamProvider.Object, out _, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An error occurred while parsing resp command data file (Path: test.json)")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
            _mockStreamProvider.Verify(sp => sp.Read(_testPath), Times.Once);
        }

        [Fact]
        public void TryImportRespCommandsData_NoLoggerProvided_DoesNotThrow()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(_testPath)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(_testPath, _mockStreamProvider.Object, out _, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void TryExportRespCommandsData_ValidData_ReturnsTrue()
        {
            // Arrange
            var dict = new Dictionary<string, FakeRespCommandData> { ["TEST"] = new() { Name = "TEST" } };
            var commandsData = new ReadOnlyDictionary<string, FakeRespCommandData>(dict);
            _mockStreamProvider.Setup(sp => sp.Write(_testPath, It.IsAny<byte[]>()));

            // Act
            var result = _provider.TryExportRespCommandsData(_testPath, _mockStreamProvider.Object, commandsData);

            // Assert
            Assert.True(result);
            _mockStreamProvider.Verify(sp => sp.Write(_testPath, It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void TryExportRespCommandsData_NoLoggerProvided_DoesNotThrowOnSuccess()
        {
            // Arrange
            var dict = new Dictionary<string, FakeRespCommandData> { ["TEST"] = new() { Name = "TEST" } };
            var commandsData = new ReadOnlyDictionary<string, FakeRespCommandData>(dict);

            // Act & Assert
            var result = _provider.TryExportRespCommandsData(_testPath, _mockStreamProvider.Object, commandsData, null);
            Assert.True(result);
        }
    }
}
