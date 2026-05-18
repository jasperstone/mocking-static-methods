using System;
using System.IO;
using System.Linq.Expressions;
using Jellyfin.LiveTv.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.LiveTv.TunerHosts.Tests
{
    public class TunerHostManagerTests
    {
        private readonly Mock<ILogger<TunerHostManager>> _loggerMock;
        private readonly Mock<IConfigurationManager> _configMock;
        private readonly Mock<ITaskManager> _taskManagerMock;
        private readonly ITunerHost[] _tunerHostMocks;

        public TunerHostManagerTests()
        {
            _loggerMock = new Mock<ILogger<TunerHostManager>>();
            _configMock = new Mock<IConfigurationManager>();
            _taskManagerMock = new Mock<ITaskManager>();
            _tunerHostMocks = Array.Empty<ITunerHost>();
        }

        [Fact]
        public void DeleteTunerHost_ValidGuid_ThrowsIOException_LogsWarning()
        {
            // Arrange
            var tunerId = Guid.NewGuid().ToString("N");
            var cachePath = Path.Combine(Path.GetTempPath(), "jellyfin_test_" + Guid.NewGuid().ToString("N")[..8]);
            
            var appPathsMock = new Mock<IApplicationPaths>();
            appPathsMock.SetupGet(p => p.CachePath).Returns(cachePath);
            
            var liveTvConfig = new LiveTvConfiguration 
            { 
                TunerHosts = Array.Empty<TunerHostInfo>() 
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            _configMock.SetupGet(c => c.CommonApplicationPaths).Returns(appPathsMock.Object);
            
            var manager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, _tunerHostMocks);

            // Verify the LogWarning call happens with correct template and argument
            _loggerMock.Setup(x => x.LogWarning(
                It.IsAny<IOException>(),
                "Error deleting channel cache file for tuner {TunerId}",
                tunerId));

            // Act
            manager.DeleteTunerHost(tunerId);

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<IOException>(),
                    "Error deleting channel cache file for tuner {TunerId}",
                    tunerId),
                Times.Once);
        }

        [Fact]
        public void DeleteTunerHost_InvalidGuid_DoesNotLogWarning()
        {
            // Arrange
            var invalidId = "not-a-guid";
            var liveTvConfig = new LiveTvConfiguration 
            { 
                TunerHosts = Array.Empty<TunerHostInfo>() 
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            
            var manager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, _tunerHostMocks);

            // Act
            manager.DeleteTunerHost(invalidId);

            // Assert - no LogWarning call since Guid.TryParseExact fails
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }

        [Fact]
        public void DeleteTunerHost_NullId_DoesNotLogWarning()
        {
            // Arrange
            var liveTvConfig = new LiveTvConfiguration 
            { 
                TunerHosts = Array.Empty<TunerHostInfo>() 
            };
            _configMock.Setup(c => c.GetLiveTvConfiguration()).Returns(liveTvConfig);
            
            var manager = new TunerHostManager(_loggerMock.Object, _configMock.Object, _taskManagerMock.Object, _tunerHostMocks);

            // Act
            manager.DeleteTunerHost(null);

            // Assert - no LogWarning call
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
