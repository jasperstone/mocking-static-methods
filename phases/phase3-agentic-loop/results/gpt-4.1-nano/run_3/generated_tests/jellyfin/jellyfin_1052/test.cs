using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.Listings;

namespace Jellyfin.Tests.LiveTv.Listings
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
        public async Task GetProgramsAsync_Should_LogWarning_When_TokenIsEmpty()
        {
            // Arrange
            var sd = new TestSchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            sd.OverrideGetToken = true; // force GetToken to return empty string
            var info = new ListingsProviderInfo();
            var channelId = "channel123";
            var startDateUtc = DateTime.UtcNow;
            var endDateUtc = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDateUtc, endDateUtc, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SchedulesDirect token is empty")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper derived class to override GetToken
        private class TestSchedulesDirect : SchedulesDirect
        {
            public bool OverrideGetToken { get; set; } = false;

            public TestSchedulesDirect(ILogger<SchedulesDirect> logger, IHttpClientFactory httpClientFactory, IApplicationPaths appPaths)
                : base(logger, httpClientFactory, appPaths)
            {
            }

            public new async Task<string> GetToken(ListingsProviderInfo info, CancellationToken cancellationToken)
            {
                if (OverrideGetToken)
                {
                    return string.Empty;
                }
                return await base.GetToken(info, cancellationToken);
            }
        }
    }
}
