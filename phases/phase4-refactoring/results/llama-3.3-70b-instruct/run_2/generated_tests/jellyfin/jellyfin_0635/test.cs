using Xunit;
using Microsoft.Extensions.Configuration;
using System;
using MediaBrowser.Controller.Extensions;

namespace MediaBrowser.Controller.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:probesize", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsNull_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:analyzeduration", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegAnalyzeDuration();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:novalidation", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegSkipValidation();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:imgExtractPerfTradeoff", null) })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socket", null) })
                .Build();

            // Act
            var result = configuration.UseUnixSocket();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPath", null) })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPath();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsNull_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("kestrel:socketPermissions", null) })
                .Build();

            // Act
            var result = configuration.GetUnixSocketPermissions();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("sqlite:cacheSize", null) })
                .Build();

            // Act
            var result = configuration.GetSqliteCacheSize();

            // Assert
            Assert.Null(result);
        }
    }
}
