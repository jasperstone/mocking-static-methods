using Xunit;
using Microsoft.Extensions.Configuration;
using System;

namespace MediaBrowser.Controller.Extensions
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_DefaultConfiguration_ReturnsFalse()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "false")
                })
                .Build();

            // Act
            var result = ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(configuration);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ConfigurationWithTrueValue_ReturnsTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "true")
                })
                .Build();

            // Act
            var result = ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(configuration);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ConfigurationWithInvalidValue_ThrowsInvalidOperationException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey, "invalid")
                })
                .Build();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(configuration));
        }
    }
}
