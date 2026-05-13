using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Channels;

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
        public async Task GetProgramsAsync_Should_LogWarningAndReturnEmpty_When_TokenIsEmpty()
        {
            // Arrange
            var sd = new SchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            var info = new ListingsProviderInfo();
            var channelId = "channel1";
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Mock GetToken to return empty string
            var sdType = typeof(SchedulesDirect);
            var getTokenMethod = sdType.GetMethod("GetToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tokenTask = Task.FromResult<string>(string.Empty);
            var mockGetToken = new Moq.Mock<SchedulesDirect>(MockBehavior.Strict, _loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            mockGetToken.Setup(m => m.GetType().GetMethod("GetToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(m.Object, It.IsAny<object[]>()))
                .Returns(tokenTask);

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDate, endDate, cancellationToken);

            // Assert
            Assert.Empty(result);
            _loggerMock.VerifyLog(LogLevel.Warning, "SchedulesDirect token is empty, returning empty program list");
        }

        [Fact]
        public async Task GetProgramsAsync_Should_LogWarning_When_TokenIsNull()
        {
            // Arrange
            var sd = new SchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            var info = new ListingsProviderInfo();
            var channelId = "channel2";
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Mock GetToken to return null
            var sdType = typeof(SchedulesDirect);
            var getTokenMethod = sdType.GetMethod("GetToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tokenTask = Task.FromResult<string>(null);
            var mockGetToken = new Moq.Mock<SchedulesDirect>(MockBehavior.Strict, _loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            mockGetToken.Setup(m => m.GetType().GetMethod("GetToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(m.Object, It.IsAny<object[]>()))
                .Returns(tokenTask);

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDate, endDate, cancellationToken);

            // Assert
            Assert.Empty(result);
            _loggerMock.VerifyLog(LogLevel.Warning, "SchedulesDirect token is empty, returning empty program list");
        }

        [Fact]
        public async Task GetProgramsAsync_Should_LogInformationAndDebug_When_ValidTokenAndSchedules()
        {
            // Arrange
            var sd = new SchedulesDirect(_loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            var info = new ListingsProviderInfo();
            var channelId = "channel3";
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(1);
            var cancellationToken = CancellationToken.None;

            // Mock GetToken to return a valid token
            var token = "valid_token";
            var getTokenTask = Task.FromResult(token);
            var mockGetToken = new Moq.Mock<SchedulesDirect>(MockBehavior.Strict, _loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            mockGetToken.Setup(m => m.GetType().GetMethod("GetToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(m.Object, It.IsAny<object[]>()))
                .Returns(getTokenTask);

            // Mock Request to return dummy schedules
            var dummySchedules = new List<DayDto>
            {
                new DayDto
                {
                    Programs = new List<ProgramDto>
                    {
                        new ProgramDto { ProgramId = "prog1", AirDateTime = DateTime.UtcNow, Duration = 3600 }
                    }
                }
            };

            var requestMethod = typeof(SchedulesDirect).GetMethod("Request", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requestMock = new Moq.Mock<SchedulesDirect>(MockBehavior.Strict, _loggerMock.Object, _httpClientFactoryMock.Object, _appPathsMock.Object);
            requestMock.Setup(m => m.GetType().GetMethod("Request", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(m.Object, It.IsAny<object[]>()))
                .ReturnsAsync(dummySchedules);

            // Act
            var result = await sd.GetProgramsAsync(info, channelId, startDate, endDate, cancellationToken);

            // Assert
            Assert.NotNull(result);
            _loggerMock.VerifyLog(LogLevel.Information, $"Channel Station ID is: {channelId}");
            _loggerMock.VerifyLog(LogLevel.Debug, "Request string for schedules is: {@RequestString}");
            _loggerMock.VerifyLog(LogLevel.Debug, "Found 1 programs on {ChannelID} ScheduleDirect", channelId);
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel level, string message)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
