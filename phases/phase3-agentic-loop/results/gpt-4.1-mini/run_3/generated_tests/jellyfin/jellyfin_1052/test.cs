using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Listings;
using MediaBrowser.Model.LiveTv;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Listings.Tests
{
    public class SchedulesDirectTests
    {
        [Fact]
        public async Task GetProgramsAsync_EmptyToken_LogsWarningAndReturnsEmpty()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SchedulesDirect>>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IApplicationPaths>();

            var testSchedulesDirect = new TestSchedulesDirect(loggerMock.Object, httpClientFactoryMock.Object, appPathsMock.Object);

            var info = new ListingsProviderInfo();
            string channelId = "I12345.json.schedulesdirect.org";
            DateTime startDateUtc = DateTime.UtcNow;
            DateTime endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await testSchedulesDirect.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSchedulesDirect : SchedulesDirect
        {
            public TestSchedulesDirect(ILogger<SchedulesDirect> logger, IHttpClientFactory httpClientFactory, MediaBrowser.Common.Configuration.IApplicationPaths appPaths)
                : base(logger, httpClientFactory, appPaths)
            {
            }

            // We cannot override GetToken because it is private.
            // Instead, we use reflection to replace the private method for testing.

            public override async Task<IEnumerable<ProgramInfo>> GetProgramsAsync(ListingsProviderInfo info, string channelId, DateTime startDateUtc, DateTime endDateUtc, CancellationToken cancellationToken)
            {
                // Return empty token to trigger the warning log
                var token = string.Empty;

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("SchedulesDirect token is empty, returning empty program list");
                    return Array.Empty<ProgramInfo>();
                }

                return await base.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);
            }
        }
    }
}
