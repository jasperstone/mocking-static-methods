using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Http;

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
            appPathsMock.SetupGet(p => p.CachePath).Returns("/cache");
            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            var info = new ListingsProviderInfo();
            var channelId = "channelId";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var token = string.Empty;
            var programs = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("SchedulesDirect token is empty, returning empty program list"), Times.Once);
        }
    }
}
