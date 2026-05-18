using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using MediaBrowser.Controller.Extensions;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnTrue_WhenKeyExistsAndValueIsTrue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(true);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnFalse_WhenKeyExistsAndValueIsFalse()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(false);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(false);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

        // Assert
        Assert.False(result);
    }
}
