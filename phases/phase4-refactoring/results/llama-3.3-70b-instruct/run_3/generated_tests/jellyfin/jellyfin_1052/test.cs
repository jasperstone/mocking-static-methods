using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Configuration;

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
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.CachePath).Returns("/cache/path");
            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);
            var info = new ListingsProviderInfo();
            var channelId = "channelId";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("SchedulesDirect token is empty, returning empty program list"), Times.Once);
        }
    }
}
