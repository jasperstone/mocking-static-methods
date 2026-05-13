using Xunit;
using Microsoft.Extensions.Configuration;
using System;

namespace MediaBrowser.Controller.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenConfigValueIsTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "true")
                })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigValueIsFalse()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false")
                })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenConfigValueIsMissing()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }
    }
}
