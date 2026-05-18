using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Model.LiveTv;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.LiveTv.Tests.Listings
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
            appPathsMock.Setup(x => x.CachePath).Returns("cache");

            var schedulesDirect = new SchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            var info = new ListingsProviderInfo();
            var channelId = "testChannel";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Mock the GetToken method to return an empty token
            var schedulesDirectMock = new Mock<SchedulesDirect>(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);
            schedulesDirectMock.Setup(x => x.GetToken(It.IsAny<ListingsProviderInfo>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

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
