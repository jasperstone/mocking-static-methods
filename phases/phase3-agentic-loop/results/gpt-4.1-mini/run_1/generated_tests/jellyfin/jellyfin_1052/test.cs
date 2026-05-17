using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Model.LiveTv
{
    // Minimal stub for ListingsProviderInfo to allow compilation
    public class ListingsProviderInfo
    {
    }
}

namespace Jellyfin.LiveTv.Listings.Tests
{
    using MediaBrowser.Model.LiveTv;

    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_EmptyToken_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();

            var testSchedulesDirect = new TestSchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            var info = new ListingsProviderInfo();
            string channelId = "I12345.json.schedulesdirect.org";
            DateTime startDateUtc = DateTime.UtcNow.Date;
            DateTime endDateUtc = startDateUtc.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await testSchedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty, returning empty program list")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSchedulesDirect : SchedulesDirect
        {
            public TestSchedulesDirect(ILogger<SchedulesDirect> logger, System.Net.Http.IHttpClientFactory httpClientFactory, MediaBrowser.Common.Configuration.IApplicationPaths appPaths)
                : base(logger, httpClientFactory, appPaths)
            {
            }

            // Hide base private method with new public method for testing
            public new Task<string> GetToken(ListingsProviderInfo info, CancellationToken cancellationToken)
            {
                return Task.FromResult(string.Empty);
            }

            // Hide base private method with new public method for testing
            public new bool IsMetadataLimitActive()
            {
                return false;
            }
        }
    }
}
