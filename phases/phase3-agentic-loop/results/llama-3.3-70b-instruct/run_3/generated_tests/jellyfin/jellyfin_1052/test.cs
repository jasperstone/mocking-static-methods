using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_LogsWarningWhenTokenIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, null);

            var channelId = "channelId";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            await schedulesDirect.GetProgramsAsync(null, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("SchedulesDirect token is empty, returning empty program list"), Times.Once);
        }
    }
}
