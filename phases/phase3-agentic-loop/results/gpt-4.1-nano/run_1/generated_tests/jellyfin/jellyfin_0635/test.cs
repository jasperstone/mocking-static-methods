using Xunit;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using System.Collections.Generic;

namespace MediaBrowser.Tests
{
    public class ConfigurationExtensionsTests
    {
        private IConfiguration CreateConfiguration(Dictionary<string, string> data)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(data)
                .Build();
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsExpectedValue()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegProbeSizeKey, "probeSizeValue" }
            });

            var result = config.GetFFmpegProbeSize();

            Assert.Equal("probeSizeValue", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsExpectedValue()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegAnalyzeDurationKey, "analyzeDurationValue" }
            });

            var result = config.GetFFmpegAnalyzeDuration();

            Assert.Equal("analyzeDurationValue", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenSetToTrue()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegSkipValidationKey, "true" }
            });

            var result = config.GetFFmpegSkipValidation();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenSetToFalse()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegSkipValidationKey, "false" }
            });

            var result = config.GetFFmpegSkipValidation();

            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenSetToTrue()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "true" }
            });

            var result = config.GetFFmpegImgExtractPerfTradeoff();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenSetToFalse()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false" }
            });

            var result = config.GetFFmpegImgExtractPerfTradeoff();

            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenSetToTrue()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.BindToUnixSocketKey, "true" }
            });

            var result = config.UseUnixSocket();

            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenSetToFalse()
        {
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.BindToUnixSocketKey, "false" }
            });

            var result = config.UseUnixSocket();

            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsExpectedValue()
        {
            var pathValue = "/tmp/socket";
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPathKey, pathValue }
            });

            var result = config.GetUnixSocketPath();

            Assert.Equal(pathValue, result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsExpectedValue()
        {
            var permValue = "0777";
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPermissionsKey, permValue }
            });

            var result = config.GetUnixSocketPermissions();

            Assert.Equal(permValue, result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsExpectedValue()
        {
            var cacheSize = 256;
            var config = CreateConfiguration(new Dictionary<string, string>
            {
                { ConfigurationExtensions.SqliteCacheSizeKey, cacheSize.ToString() }
            });

            var result = config.GetSqliteCacheSize();

            Assert.Equal(cacheSize, result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsNull_WhenNotSet()
        {
            var config = CreateConfiguration(new Dictionary<string, string>());

            var result = config.GetSqliteCacheSize();

            Assert.Null(result);
        }
    }
}
