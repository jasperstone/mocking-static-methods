using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeySetToTrue()
        {
            var data = new Dictionary<string, string>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenKeySetToFalse()
        {
            var data = new Dictionary<string, string>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "false"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenKeySetTo1()
        {
            var data = new Dictionary<string, string>
            {
                ["FFmpeg:imgExtractPerfTradeoff"] = "1"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.GetFFmpegImgExtractPerfTradeoff();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.GetFFmpegSkipValidation();
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenKeySetToTrue()
        {
            var data = new Dictionary<string, string>
            {
                ["FFmpeg:novalidation"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.GetFFmpegSkipValidation();
            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.UseUnixSocket();
            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenKeySetToTrue()
        {
            var data = new Dictionary<string, string>
            {
                ["kestrel:socket"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.UseUnixSocket();
            Assert.True(result);
        }

        [Fact]
        public void HostWebClient_ReturnsFalse_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            bool result = config.HostWebClient();
            Assert.False(result);
        }

        [Fact]
        public void HostWebClient_ReturnsTrue_WhenKeySetToTrue()
        {
            var data = new Dictionary<string, string>
            {
                ["hostwebclient"] = "true"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            bool result = config.HostWebClient();
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsNull_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            string? result = config.GetFFmpegProbeSize();
            Assert.Null(result);
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsValue_WhenKeyPresent()
        {
            var data = new Dictionary<string, string>
            {
                ["FFmpeg:probesize"] = "10M"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            string? result = config.GetFFmpegProbeSize();
            Assert.Equal("10M", result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsNull_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            string? result = config.GetUnixSocketPath();
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenKeyMissing()
        {
            var config = new ConfigurationBuilder().Build();
            int? result = config.GetSqliteCacheSize();
            Assert.Null(result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsValue_WhenKeyPresent()
        {
            var data = new Dictionary<string, string>
            {
                ["sqlite:cacheSize"] = "10000"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
            int? result = config.GetSqliteCacheSize();
            Assert.Equal(10000, result);
        }
    }
}
