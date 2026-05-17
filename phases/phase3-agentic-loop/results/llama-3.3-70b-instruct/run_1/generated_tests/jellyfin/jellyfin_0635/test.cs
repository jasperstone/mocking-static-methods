using Microsoft.Extensions.Configuration;
using Xunit;

namespace MediaBrowser.Controller.Extensions.Tests
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsDefaultValue_WhenSettingIsNotPresent()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("OtherSetting", "OtherValue") })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsTrue_WhenSettingIsPresentAndTrue()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:imgExtractPerfTradeoff", "true") })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetFFmpegImgExtractPerfTradeoff_ReturnsFalse_WhenSettingIsPresentAndFalse()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("FFmpeg:imgExtractPerfTradeoff", "false") })
                .Build();

            // Act
            var result = configuration.GetFFmpegImgExtractPerfTradeoff();

            // Assert
            Assert.False(result);
        }
    }
}
