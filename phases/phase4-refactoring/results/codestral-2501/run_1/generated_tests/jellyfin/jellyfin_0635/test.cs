using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using MediaBrowser.Controller.Extensions;

public class ConfigurationExtensionsTests
{
    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnTrue_WhenConfigValueIsTrue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(true);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetFFmpegImgExtractPerfTradeoff_ShouldReturnFalse_WhenConfigValueIsFalse()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegImgExtractPerfTradeoffKey)).Returns(false);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegImgExtractPerfTradeoff(mockConfiguration.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFFmpegProbeSize_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[ConfigurationExtensions.FfmpegProbeSizeKey]).Returns("12345");

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegProbeSize(mockConfiguration.Object);

        // Assert
        Assert.Equal("12345", result);
    }

    [Fact]
    public void GetFFmpegAnalyzeDuration_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[ConfigurationExtensions.FfmpegAnalyzeDurationKey]).Returns("67890");

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegAnalyzeDuration(mockConfiguration.Object);

        // Assert
        Assert.Equal("67890", result);
    }

    [Fact]
    public void GetFFmpegSkipValidation_ShouldReturnTrue_WhenConfigValueIsTrue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegSkipValidationKey)).Returns(true);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegSkipValidation(mockConfiguration.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetFFmpegSkipValidation_ShouldReturnFalse_WhenConfigValueIsFalse()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.FfmpegSkipValidationKey)).Returns(false);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetFFmpegSkipValidation(mockConfiguration.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void UseUnixSocket_ShouldReturnTrue_WhenConfigValueIsTrue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.BindToUnixSocketKey)).Returns(true);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.UseUnixSocket(mockConfiguration.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UseUnixSocket_ShouldReturnFalse_WhenConfigValueIsFalse()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<bool>(ConfigurationExtensions.BindToUnixSocketKey)).Returns(false);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.UseUnixSocket(mockConfiguration.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetUnixSocketPath_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[ConfigurationExtensions.UnixSocketPathKey]).Returns("/var/run/jellyfin.sock");

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetUnixSocketPath(mockConfiguration.Object);

        // Assert
        Assert.Equal("/var/run/jellyfin.sock", result);
    }

    [Fact]
    public void GetUnixSocketPermissions_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c[ConfigurationExtensions.UnixSocketPermissionsKey]).Returns("0660");

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetUnixSocketPermissions(mockConfiguration.Object);

        // Assert
        Assert.Equal("0660", result);
    }

    [Fact]
    public void GetSqliteCacheSize_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c.GetValue<int?>(ConfigurationExtensions.SqliteCacheSizeKey)).Returns(2000);

        // Act
        var result = MediaBrowser.Controller.Extensions.ConfigurationExtensions.GetSqliteCacheSize(mockConfiguration.Object);

        // Assert
        Assert.Equal(2000, result);
    }
}
