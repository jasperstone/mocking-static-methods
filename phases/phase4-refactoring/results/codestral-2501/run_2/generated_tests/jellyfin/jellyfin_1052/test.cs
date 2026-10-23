using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Common.Configuration;
using System.Net.Http;
using System.Collections.Generic;
using Jellyfin.LiveTv.Listings.SchedulesDirectDtos;
using System.Globalization;

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

        [Fact]
        public async Task GetProgramsAsync_NullDailySchedules_ReturnsEmptyProgramList()
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

            var requestList = new List<RequestScheduleForChannelDto>()
            {
                new()
                {
                    StationId = channelId,
                    Date = new List<string> { startDateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
                }
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProgramsAsync_NullProgramDetails_ReturnsEmptyProgramList()
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

            var requestList = new List<RequestScheduleForChannelDto>()
            {
                new()
                {
                    StationId = channelId,
                    Date = new List<string> { startDateUtc.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }
                }
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[]")
                });

            var httpClient = new HttpClient(httpMessageHandlerMock.Object);
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await schedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
        }
    }
}
