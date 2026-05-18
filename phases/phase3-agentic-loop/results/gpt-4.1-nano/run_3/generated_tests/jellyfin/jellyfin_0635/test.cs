using Xunit;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Controller.Extensions;
using System.Collections.Generic;

namespace MediaBrowser.Tests
{
    public class ConfigurationExtensionsTests
    {
        private IConfiguration CreateConfiguration(Dictionary<string, string> settings)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void GetFFmpegProbeSize_ReturnsExpectedValue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegProbeSizeKey, "probeSizeValue" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegProbeSize();

            Assert.Equal("probeSizeValue", result);
        }

        [Fact]
        public void GetFFmpegAnalyzeDuration_ReturnsExpectedValue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegAnalyzeDurationKey, "analyzeDurationValue" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegAnalyzeDuration();

            Assert.Equal("analyzeDurationValue", result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsTrue_WhenSettingIsTrue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegSkipValidationKey, "true" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegSkipValidation();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegSkipValidation_ReturnsFalse_WhenSettingIsFalse()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegSkipValidationKey, "false" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegSkipValidation();

            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenSettingIsTrue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "true" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegImgExtractPerfTradeoff();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenSettingIsFalse()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetFFmpegImgExtractPerfTradeoff();

            Assert.False(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsTrue_WhenSettingIsTrue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.BindToUnixSocketKey, "true" }
            };
            var config = CreateConfiguration(settings);

            var result = config.UseUnixSocket();

            Assert.True(result);
        }

        [Fact]
        public void UseUnixSocket_ReturnsFalse_WhenSettingIsFalse()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.BindToUnixSocketKey, "false" }
            };
            var config = CreateConfiguration(settings);

            var result = config.UseUnixSocket();

            Assert.False(result);
        }

        [Fact]
        public void GetUnixSocketPath_ReturnsExpectedValue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPathKey, "/tmp/socket" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetUnixSocketPath();

            Assert.Equal("/tmp/socket", result);
        }

        [Fact]
        public void GetUnixSocketPermissions_ReturnsExpectedValue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.UnixSocketPermissionsKey, "0777" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetUnixSocketPermissions();

            Assert.Equal("0777", result);
        }

        [Fact]
        public void GetSqliteCacheSize_ReturnsExpectedValue()
        {
            var settings = new Dictionary<string, string>
            {
                { ConfigurationExtensions.SqliteCacheSizeKey, "100" }
            };
            var config = CreateConfiguration(settings);

            var result = config.GetSqliteCacheSize();

            Assert.Equal(100, result);
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
