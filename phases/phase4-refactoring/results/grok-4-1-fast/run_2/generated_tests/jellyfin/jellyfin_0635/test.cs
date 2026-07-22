using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigValueIsFalse()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "false"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyMissing()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>();
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ThrowsInvalidOperationException_WhenConfigValueIsInvalid()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "invalid"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["FFmpeg:novalidation"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetFFmpegSkipValidation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["kestrel:socket"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.UseUnixSocket();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HostWebClient_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var configDict = new Dictionary<string, string?>
            {
                ["hostwebclient"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.HostWebClient();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyPresent()
        {
            // Arrange
            var expectedValue = "5000000";
            var configDict = new Dictionary<string, string?>
            {
                ["FFmpeg:probesize"] = expectedValue
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetFFmpegProbeSize();

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();

            // Act
            var result = config.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValue_WhenValidIntPresent()
        {
            // Arrange
            var expectedValue = "1000";
            var configDict = new Dictionary<string, string?>
            {
                ["sqlite:cacheSize"] = expectedValue
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDict)
                .Build();

            // Act
            var result = config.GetSqliteCacheSize();

            // Assert
            Assert.Equal(1000, result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();

            // Act
            var result = config.GetSqliteCacheSize();

            // Assert
            Assert.Null(result);
        }
    }
}
