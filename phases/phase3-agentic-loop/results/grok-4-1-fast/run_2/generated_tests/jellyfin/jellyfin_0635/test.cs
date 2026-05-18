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
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenValueTrue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "true"
                })
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenValueFalse()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "false"
                })
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenValue0()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "0"
                })
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenValue1()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "1"
                })
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ThrowsInvalidOperationException_WhenInvalidValue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "invalid"
                })
                .Build();
            var exception = Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
            Assert.Contains("Failed to convert configuration value", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        // Additional coverage for other GetValue<bool> methods

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.GetFFmpegSkipValidation();
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.UseUnixSocket();
            Assert.False(result);
        }

        [Fact]
        public void HostWebClient_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.HostWebClient();
            Assert.False(result);
        }
    }
}
