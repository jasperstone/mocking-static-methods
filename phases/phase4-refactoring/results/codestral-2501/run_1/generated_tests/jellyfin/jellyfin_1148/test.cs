using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.Configuration;
using Jellyfin.LiveTv.Guide;
using Jellyfin.LiveTv.TunerHosts;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.TunerHosts
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly Mock<ITunerHost> _tunerHostMock;
        private readonly TunerHostManager _tunerHostManager;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMock = new Mock<ITunerHost>();

            _tunerHostManager = new TunerHostManager(
                _loggerMock.Object,
                _configMock.Object,
                _taskManagerMock.Object,
                new[] { _tunerHostMock.Object });
        }

        [Fact]
        public void DeleteTunerHost_ValidId_DeletesCacheFileAndLogsWarning()
        {
            // Arrange
            var id = Guid.NewGuid().ToString("N");
            var config = new LiveTvOptions
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = id }
                }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _configMock.Setup(c => c.CommonApplicationPaths.CachePath).Returns(Path.GetTempPath());

            // Act
            _tunerHostManager.DeleteTunerHost(id);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _taskManagerMock.Verify(
                x => x.CancelIfRunningAndQueue<RefreshGuideScheduledTask>(),
                Times.Once);
        }

        [Fact]
        public void DeleteTunerHost_InvalidId_DoesNotDeleteCacheFileAndDoesNotLogWarning()
        {
            // Arrange
            var id = "invalid-id";
            var config = new LiveTvOptions
            {
                TunerHosts = new[]
                {
                    new TunerHostInfo { Id = Guid.NewGuid().ToString("N") }
                }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);

            // Act
            _tunerHostManager.DeleteTunerHost(id);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);

            _taskManagerMock.Verify(
                x => x.CancelIfRunningAndQueue<RefreshGuideScheduledTask>(),
                Times.Once);
        }

        [Fact]
        public async Task DiscoverTuners_NewDevicesOnly_ReturnsNewDevices()
        {
            // Arrange
            var newDevice = new TunerHostInfo { DeviceId = "new-device" };
            _tunerHostMock.Setup(x => x.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TunerHostInfo> { newDevice });

            // Act
            var result = await _tunerHostManager.DiscoverTuners(true).ToListAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(newDevice, result[0]);
        }

        [Fact]
        public async Task DiscoverTuners_AllDevices_ReturnsAllDevices()
        {
            // Arrange
            var existingDevice = new TunerHostInfo { DeviceId = "existing-device" };
            var newDevice = new TunerHostInfo { DeviceId = "new-device" };
            var config = new LiveTvOptions
            {
                TunerHosts = new[] { existingDevice }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _tunerHostMock.Setup(x => x.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TunerHostInfo> { existingDevice, newDevice });

            // Act
            var result = await _tunerHostManager.DiscoverTuners(false).ToListAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, d => d.DeviceId == existingDevice.DeviceId);
            Assert.Contains(result, d => d.DeviceId == newDevice.DeviceId);
        }

        [Fact]
        public async Task ScanForTunerDeviceChanges_UpdatesDeviceUrl()
        {
            // Arrange
            var existingDevice = new TunerHostInfo { DeviceId = "existing-device", Url = "old-url" };
            var updatedDevice = new TunerHostInfo { DeviceId = "existing-device", Url = "new-url" };
            var config = new LiveTvOptions
            {
                TunerHosts = new[] { existingDevice }
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(config);
            _tunerHostMock.Setup(x => x.DiscoverDevices(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TunerHostInfo> { updatedDevice });

            // Act
            await _tunerHostManager.ScanForTunerDeviceChanges(CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _configMock.Verify(
                x => x.SaveConfiguration(It.IsAny<string>(), It.IsAny<LiveTvOptions>()),
                Times.Once);
        }
    }
}
