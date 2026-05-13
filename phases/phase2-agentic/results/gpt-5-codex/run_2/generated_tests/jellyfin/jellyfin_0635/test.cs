using System.Collections.Generic;
using MediaBrowser.Controller.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaBrowser.Controller.Tests.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenConfigurationValueIsTrue()
        {
            var configuration = BuildConfiguration((ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "true"));

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigurationValueIsFalse()
        {
            var configuration = BuildConfiguration((ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false"));

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigurationValueIsMissing()
        {
            var configuration = BuildConfiguration();

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.False(result);
        }

        private static IConfiguration BuildConfiguration(params (string Key, string Value)[] entries)
        {
            var dictionary = new Dictionary<string, string?>();
            foreach (var entry in entries)
            {
                dictionary[entry.Key] = entry.Value;
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(dictionary)
                .Build();
        }
    }
}
