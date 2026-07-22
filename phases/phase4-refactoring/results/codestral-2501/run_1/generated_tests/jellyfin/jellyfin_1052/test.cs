using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Model.LiveTv;
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

            appPathsMock.Setup(x => x.CachePath).Returns("fake/path");

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
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty, returning empty program list")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
