using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests
    {
        private readonly Mock<ILogger<SchedulesDirect>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<IApplicationPaths>();
        }

        [Fact]
        public async Task GetProgramsAsync_LogsWarningWhenTokenIsEmpty()
        {
            // Arrange
            var schedulesDirect = new SchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            var listingsProviderInfo = new ListingsProviderInfo();
            var channelId = "channelId";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await schedulesDirect.GetProgramsAsync(listingsProviderInfo, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning("SchedulesDirect token is empty, returning empty program list"), Times.Once);
        }
    }
}
