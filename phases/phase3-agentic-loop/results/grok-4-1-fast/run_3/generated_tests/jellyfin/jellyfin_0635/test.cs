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
        public void GetFFmpegImgExtractPerfTradeoff_ThrowsInvalidOperationException_WhenInvalidValue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:imgExtractPerfTradeoff"] = "invalid"
                })
                .Build();
            var ex = Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
            Assert.Contains("Failed to convert configuration value", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.GetFFmpegSkipValidation();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenValueTrue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["FFmpeg:novalidation"] = "true"
                })
                .Build();
            bool result = config.GetFFmpegSkipValidation();
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.UseUnixSocket();
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenValueTrue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["kestrel:socket"] = "true"
                })
                .Build();
            bool result = config.UseUnixSocket();
            Assert.True(result);
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
