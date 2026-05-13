using System.Collections.Generic;
using MediaBrowser.Controller.Extensions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaBrowser.Controller.Tests.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("TRUE", true)]
        [InlineData("FALSE", false)]
        public void GetFFmpegImgExtractPerfTradeoff_ReadsConfiguredBoolean(string value, bool expected)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey] = value
                })
                .Build();

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalseWhenValueNotProvided()
        {
            var configuration = new ConfigurationBuilder().Build();

            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            Assert.False(result);
        }
    }
}
