using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests
{
    public class CalendarPluginTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockConnector = new Mock<ICalendarConnector>();
            var plugin = new CalendarPlugin(mockConnector.Object, null, null);

            // Act
            await plugin.GetCalendarEventsAsync(5, 2, CancellationToken.None);

            // Assert
            mockLogger.Verify(
                x => x.LogDebug(
                    It.Is<string>(s => s.Contains("Getting calendar events with query options top: '5', skip:'2'.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
