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
            foreach (var kvp in values)
            {
                builder.AddInMemoryCollection(new[] { new KeyValuePair<string, string?>(kvp.Key, kvp.Value) });
            }
            return builder.Build();
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenConfiguredTrue()
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
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfiguredFalse()
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
            var config = CreateConfiguration(new Dictionary<string, string>());

            // Act
            var result = config.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ThrowsInvalidOperationException_WhenInvalidValue()
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
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenConfiguredTrue()
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
        public void UseUnixSocket_ReturnsTrue_WhenConfiguredTrue()
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
        public void HostWebClient_ReturnsTrue_WhenConfiguredTrue()
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
    }
}
