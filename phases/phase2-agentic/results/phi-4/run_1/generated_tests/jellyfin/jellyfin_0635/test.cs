using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using MediaBrowser.Controller.Extensions;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldCallGetValueWithCorrectKey()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var expectedKey = ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey;
        configurationMock.Setup(c => c.GetValue<bool>(expectedKey)).Returns(true);

        // Act
        var result = configurationMock.Object.GetFFmpegImgExtractPerfTradeoff();

        // Assert
        Assert.True(result);
        configurationMock.Verify(c => c.GetValue<bool>(expectedKey), Times.Once);
    }

    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnFalse_WhenKeyNotSet()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var expectedKey = ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey;
        configurationMock.Setup(c => c.GetValue<bool>(expectedKey)).Returns(false);

        // Act
        var result = configurationMock.Object.GetFFmpegImgExtractPerfTradeoff();

        // Assert
        Assert.False(result);
        configurationMock.Verify(c => c.GetValue<bool>(expectedKey), Times.Once);
    }
}
