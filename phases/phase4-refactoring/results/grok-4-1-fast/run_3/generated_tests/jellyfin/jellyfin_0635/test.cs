using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        private IConfiguration CreateConfiguration(Dictionary<string, string?> values)
        {
            var builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(values);
            return builder.Build();
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "true"
            });

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigValueIsFalse()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "false"
            });

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyMissing()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ThrowsInvalidOperationException_WhenConfigValueIsInvalid()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "invalid"
            });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["FFmpeg:novalidation"] = "true"
            });

            // Act
            var result = config.GetFFmpegSkipValidation();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["kestrel:socket"] = "true"
            });

            // Act
            var result = config.UseUnixSocket();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HostWebClient_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["hostwebclient"] = "true"
            });

            // Act
            var result = config.HostWebClient();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyExists()
        {
            // Arrange
            var expected = "5000000";
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["FFmpeg:probesize"] = expected
            });

            // Act
            var result = config.GetFFmpegProbeSize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            var result = config.GetFFmpegProbeSize();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValue_WhenValidInt()
        {
            // Arrange
            var expected = 1000;
            var config = CreateConfiguration(new Dictionary<string, string?>
            {
                ["sqlite:cacheSize"] = "1000"
            });

            // Act
            var result = config.GetSqliteCacheSize();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenKeyMissing()
        {
            // Arrange
            var config = CreateConfiguration(new Dictionary<string, string?>());

            // Act
            var result = config.GetSqliteCacheSize();

            // Assert
            Assert.Null(result);
        }
    }
}
