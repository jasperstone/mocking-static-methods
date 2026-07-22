using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SchedulesDirectTests
{
    [Fact]
    public async Task GetProgramsAsync_LogsWarning_WhenTokenIsEmpty()
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
        loggerMock.Verify(
            l => l.LogWarning("SchedulesDirect token is empty, returning empty program list"),
            Times.Once);
        Assert.Empty(result);
    }
}
