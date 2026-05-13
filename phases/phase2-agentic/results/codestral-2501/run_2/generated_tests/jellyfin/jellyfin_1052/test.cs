using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.LiveTv.Listings;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.LiveTv;
using System.Collections.Generic;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_EmptyToken_ReturnsEmptyProgramList()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var appPathsMock = new Mock<IApplicationPaths>();

            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            var info = new ListingsProviderInfo();
            var channelId = "testChannel";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.LogWarning("SchedulesDirect token is empty, returning empty program list"),
                Times.Once);
        }
    }
}
