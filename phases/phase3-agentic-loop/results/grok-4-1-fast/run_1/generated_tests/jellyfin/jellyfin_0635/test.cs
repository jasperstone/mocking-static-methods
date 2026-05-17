using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Xunit;
using System.Collections.Generic;

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
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeyIsTrue()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FFmpeg:imgExtractPerfTradeoff", "true")
                })
                .Build();
            
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyIsFalse()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FFmpeg:imgExtractPerfTradeoff", "false")
                })
                .Build();
            
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_Throws_WhenKeyIsInvalidNumeric()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FFmpeg:imgExtractPerfTradeoff", "1")
                })
                .Build();
            
            Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_Throws_WhenKeyIsZero()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FFmpeg:imgExtractPerfTradeoff", "0")
                })
                .Build();
            
            Assert.Throws<InvalidOperationException>(() => config.GetFFmpegImgExtractPerfTradeoff());
        }

        // Additional coverage for other methods using GetValue<bool>
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

        // Coverage for string indexer methods
        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            string? result = config.GetFFmpegProbeSize();
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyPresent()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>("FFmpeg:probesize", "5000000")
                })
                .Build();
            
            string? result = config.GetFFmpegProbeSize();
            Assert.Equal("5000000", result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenKeyNotPresent()
        {
            var config = new ConfigurationBuilder().Build();
            string? result = config.GetUnixSocketPath();
            Assert.Null(result);
        }
    }
}
