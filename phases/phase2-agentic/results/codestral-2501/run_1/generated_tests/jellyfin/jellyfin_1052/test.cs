using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Jellyfin.LiveTv.Listings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.LiveTv;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests
    {
        private readonly Mock<ILogger<SchedulesDirect>> _loggerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly SchedulesDirect _schedulesDirect;

        public SchedulesDirectTests()
        {
            _loggerMock = new Mock<ILogger<SchedulesDirect>>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _appPathsMock = new Mock<IApplicationPaths>();
            _schedulesDirect = new SchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
        }

        [Fact]
        public async Task GetProgramsAsync_EmptyToken_ReturnsEmptyProgramList()
        {
            // Arrange
            var info = new ListingsProviderInfo();
            var channelId = "testChannel";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = new CancellationToken();

            _loggerMock.Setup(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            // Act
            var result = await _schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(x => x.LogWarning("SchedulesDirect token is empty, returning empty program list", It.IsAny<object[]>()), Times.Once);
        }
    }
}
