using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.Listings
{
    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_LogsWarningWhenTokenIsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var appPathsMock = new Mock<IApplicationPaths>();
            var listingsProviderInfo = new ListingsProviderInfo();
            var channelId = "channelId";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            // Act
            await schedulesDirect.GetProgramsAsync(listingsProviderInfo, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("SchedulesDirect token is empty, returning empty program list"), Times.Once);
        }
    }
}
