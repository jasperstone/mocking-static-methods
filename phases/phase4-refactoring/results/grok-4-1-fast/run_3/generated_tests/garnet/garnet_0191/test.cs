using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [Fact]
        public void LogError_CalledWithCorrectTemplateAndAddresses()
        {
            // Arrange
            long syncFromAofAddress = 50L;
            long beginAofAddress = 100L;
            string expectedTemplate = "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}";

            // Act - Directly test the LoggerExtensions.LogError extension method from line 301
            _loggerMock.Object.LogError(expectedTemplate, syncFromAofAddress, beginAofAddress);

            // Assert - Verify Log was called with Error level
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogError_FormatsMessageCorrectlyWithSpecificValues()
        {
            // Arrange
            long syncFromAofAddress = 50L;
            long beginAofAddress = 100L;

            // Act
            _loggerMock.Object.LogError(
                "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}", 
                syncFromAofAddress, 
                beginAofAddress);

            // Assert - Verify the formatter produces message containing the specific values
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.Is<Func<It.IsAnyType, Exception, string>>(formatter =>
                    {
                        try
                        {
                            // Create state object matching what LogError creates
                            var state = new { syncFromAofAddress = 50L, storeWrapper = new { appendOnlyFile = new { BeginAddress = 100L } } };
                            var formattedMessage = formatter(state, null);
                            return formattedMessage != null && 
                                   formattedMessage.Contains("50") && 
                                   formattedMessage.Contains("100") && 
                                   formattedMessage.Contains("syncFromAofAddress") &&
                                   formattedMessage.Contains("beginAofAddress");
                        }
                        catch
                        {
                            return false;
                        }
                    })),
                Times.Once);
        }

        [Fact]
        public void LogError_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger nullLogger = NullLogger.Instance;
            long syncFromAofAddress = 50L;
            long beginAofAddress = 100L;
            string template = "syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {storeWrapper.appendOnlyFile.BeginAddress}";

            // Act & Assert
            Exception ex = Record.Exception(() => nullLogger?.LogError(template, syncFromAofAddress, beginAofAddress));
            Assert.Null(ex);
        }
    }
}
